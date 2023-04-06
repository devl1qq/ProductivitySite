using API.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;




namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetValue<string>("PostgreSQL:ConnectionString");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegistrationModel registrationModel)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if the username already exists in the database
                    NpgsqlCommand checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection);
                    checkCommand.Parameters.AddWithValue("Username", registrationModel.Username);
                    long count = (long)await checkCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        return BadRequest("Username already exists");
                    }

                    // Add the new user to the database
                    NpgsqlCommand insertCommand = new NpgsqlCommand("INSERT INTO Users (Username, Password) VALUES (@Username, @Password)", connection);
                    insertCommand.Parameters.AddWithValue("Username", registrationModel.Username);
                    insertCommand.Parameters.AddWithValue("Password", BCrypt.Net.BCrypt.HashPassword(registrationModel.Password));
                    await insertCommand.ExecuteNonQueryAsync();
                }
                return Ok();
            }
            catch (NpgsqlException ex)
            {
                // Handle PostgreSQL errors here
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other errors here
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new NpgsqlCommand("SELECT UserId, Password FROM Users WHERE Username = @Username", connection);
                    command.Parameters.AddWithValue("Username", loginModel.Username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var userId = reader.GetInt32(0);
                            var password = reader.GetString(1);

                            if (BCrypt.Net.BCrypt.Verify(loginModel.Password, password))
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, loginModel.Username),
                                    new Claim("UserId", userId.ToString())
                                };

                                var jwt = new JwtSecurityToken(
                                    issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                                    audience: _configuration.GetValue<string>("Jwt:Audience"),
                                    claims: claims,
                                    expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiresInMinutes")),
                                    signingCredentials: new SigningCredentials(
                                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:SecretKey"))),
                                        SecurityAlgorithms.HmacSha256));

                                return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(jwt), UserId = userId });
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                // Handle PostgreSQL errors here
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other errors here
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return Unauthorized();
        }
    }
}
