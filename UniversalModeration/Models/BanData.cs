using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalModeration.Models
{
    public class BanData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Reason { get; set; }
        public DateTime BanDate { get; set; }
        public DateTime UnbanDate { get; set; }

        public string UserId { get; set; }
        public string PunisherId { get; set; }


        [ForeignKey(nameof(UserId))]
        public virtual UserData User { get; set; }

        [ForeignKey(nameof(PunisherId))]
        public virtual UserData Punisher { get; set; }

        [NotMapped]
        public bool IsBanned
        {
            get
            {
                return UnbanDate > DateTime.Now;
            }
        }
    }
}
