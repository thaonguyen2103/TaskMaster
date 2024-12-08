using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Models
{
    // Comment Model
    public class Comment
    {
        public string Task_ID { get; set; }
        public string User_ID { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
    }
}
