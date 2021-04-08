using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalModeration.Models
{
    public class Warn
    {
        public string? userId { get; set; }
        public string? punisherId { get; set; }
        public string? warnReason { get; set; }
        public DateTime warnDateTime { get; set; }
    }
}
