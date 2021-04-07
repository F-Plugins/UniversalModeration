using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalModeration.Models
{
    public class DiscordMessage
    {
        public List<Embed>? embeds { get; set; }
    }

    public class Embed
    {
        public string? title { get; set; }
        public int color { get; set; }
        public string? timestamp { get; set; }
        public List<Field>? fields { get; set; }
    }

    public class Field
    {
        public string? name { get; set; }
        public string? value { get; set; }
        public bool inline { get; set; }
    }
}
