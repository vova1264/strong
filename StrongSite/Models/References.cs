using System.ComponentModel.DataAnnotations.Schema;

namespace StrongSite.Models
{
    public class References
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User_Id { get; set; }
        public int CardId { get; set; }
        [ForeignKey("CardId")]
        public Cards Cards_Id { get; set; }
        public int IdPlatform { get; set; }
        public string Link { get; set; }
    }
}