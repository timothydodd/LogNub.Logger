using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogNub.Logger
{
    public class LogEntry
    {
        public int LogEntryId { get; set; }
        public string ApiKey { get; set; }
        public string Type { get; set; }
        public string Machine { get; set; }
        public string Application { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
