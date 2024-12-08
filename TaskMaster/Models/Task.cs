using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Models
{
    public class _Task
    {
        public string Task_ID { get; set; }
        public string Project_ID { get; set; }
        public string Content { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Done { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }

    }
}
