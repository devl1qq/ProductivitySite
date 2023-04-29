using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using API.Models;
using Task = API.Models.Task;
using DocumentFormat.OpenXml.Packaging;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Wordprocessing;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public TaskController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetValue<string>("PostgreSQL:ConnectionString");
        }

        [HttpGet("tasks/{userId}")]
        public async Task<ActionResult<IEnumerable<Task>>> GetAllTasks(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand("SELECT * FROM Tasks WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("UserId", userId);

                var reader = await command.ExecuteReaderAsync();
                var tasks = new List<Task>();

                while (await reader.ReadAsync())
                {
                    tasks.Add(new Task
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Description = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        Completed = reader.GetBoolean(4)
                    });
                }

                return tasks;
            }
        }


        [HttpPost("users/{userId}/tasks")]
        public async Task<ActionResult<Task>> CreateTaskForUser(int userId, [FromBody] Task task)
        {
            if (task == null || string.IsNullOrEmpty(task.Description))
            {
                return BadRequest("Task description is required.");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    "INSERT INTO Tasks (UserId, Description, CreatedAt, Completed) VALUES (@UserId, @Description, @CreatedAt, @Completed) RETURNING Id",
                    connection);
                command.Parameters.AddWithValue("UserId", userId);
                command.Parameters.AddWithValue("Description", task.Description);
                command.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("Completed", false);

                var newTaskId = await command.ExecuteScalarAsync() as int?;

                if (newTaskId == null)
                {
                    return BadRequest("Failed to create task.");
                }

                var createdTask = new Task
                {
                    Id = newTaskId.Value,
                    UserId = userId,
                    Description = task.Description,
                    CreatedAt = DateTime.UtcNow,
                    Completed = false
                };

                return Ok(createdTask);
            }
        }
        [HttpDelete("{userId}/tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(int userId, int taskId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new NpgsqlCommand("DELETE FROM Tasks WHERE Id = @Id AND UserId = @UserId", connection);
                command.Parameters.AddWithValue("Id", taskId);
                command.Parameters.AddWithValue("UserId", userId);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        [HttpPut("{userId}/tasks/{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int userId, int taskId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if task exists for the user
                var taskCommand = new NpgsqlCommand("SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId", connection);
                taskCommand.Parameters.AddWithValue("Id", taskId);
                taskCommand.Parameters.AddWithValue("UserId", userId);

                using (var reader = await taskCommand.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        return NotFound();
                    }
                }

                // Mark task as completed
                var completeCommand = new NpgsqlCommand("UPDATE Tasks SET Completed = true WHERE Id = @Id AND UserId = @UserId", connection);
                completeCommand.Parameters.AddWithValue("Id", taskId);
                completeCommand.Parameters.AddWithValue("UserId", userId);

                var rowsAffected = await completeCommand.ExecuteNonQueryAsync();

                if (rowsAffected == 1)
                {
                    return NoContent();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
        }

        [HttpPost("ResetTasks")]
        
        public async Task<IActionResult> ResetTasks(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    

                    var completedTasks = new List<Task>();
                    var command = new NpgsqlCommand("SELECT Id, Description FROM Tasks WHERE UserId = @UserId AND Completed = true", connection);
                    command.Parameters.AddWithValue("UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            completedTasks.Add(new Task
                            {
                                Id = reader.GetInt32(0),
                                Description = reader.GetString(1)
                            });
                        }
                    }
                    command = new NpgsqlCommand("UPDATE Tasks SET Completed = false WHERE UserId = @UserId AND Completed = true", connection);
                    command.Parameters.AddWithValue("UserId", userId);
                    await command.ExecuteNonQueryAsync();

                    if (completedTasks.Any())
                    {
                       return Ok(completedTasks);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}