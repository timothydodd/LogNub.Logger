using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogNub.Logger
{
    public class LogEntry
    {
        public string Type { get; set; }
        public string Machine { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
