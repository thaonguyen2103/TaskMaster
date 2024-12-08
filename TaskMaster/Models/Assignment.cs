using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Models
{
    // Assignment Model
    public class Assignment
    {
        public string Task_ID { get; set; }
        public string User_ID { get; set; }
        public int Percent { get; set; }
        public string JoinDate { get; set; }
        public string Source {  get; set; }
    }
}
