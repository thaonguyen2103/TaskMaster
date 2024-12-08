using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Models
{
    // Note Model
    public class Note
    {
        public string Note_ID { get; set; }
        public string Project_ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
    }
}
