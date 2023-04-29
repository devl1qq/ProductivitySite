using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Task
    {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool Completed { get; set; }
    }
}