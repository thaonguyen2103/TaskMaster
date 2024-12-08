using System;
namespace TaskMaster.Models
{
    public class User
    {
        public string User_ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public DateTime? DOB { get; set; }
    }
}
