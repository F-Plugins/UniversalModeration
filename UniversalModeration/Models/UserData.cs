using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalModeration.Models
{
    public class UserData
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual List<BanData> Bans { get; set; }
    }
}
