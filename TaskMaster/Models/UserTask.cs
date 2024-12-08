using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.Views;

namespace TaskMaster.Models
{
    public class UserTask
    {
        public string Title { get; set; }
        public string Source { get; set; } // The task can come from the project or be created by an individual
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
