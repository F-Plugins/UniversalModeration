using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalModeration.Models
{
    public class Ban
    {
        public string? userId { get; set; }
        public string? punisherId { get; set; }
        public string? banReason { get; set; }
        public DateTime expireDateTime { get; set; }
        public DateTime banDateTime { get; set; }
    }
}
