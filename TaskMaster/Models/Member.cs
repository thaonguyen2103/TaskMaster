using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Models
{
    // Member Model
    public class Member
    {
        public string User_ID { get; set; }
        public string Project_ID { get; set; }
        public string Role { get; set; }
        public string JoinDate { get; set; }
    }
}
