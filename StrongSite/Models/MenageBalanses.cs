using System.ComponentModel.DataAnnotations;

namespace StrongSite.Models
{
    public class MenageBalanses
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User_Id { get; set; }
        public int Amount { get; set; }
        public int MaxAmount { get; set; }
        public int MarginAmount { get; set; }
        public int CardsId { get; set; }
        public Cards Cards_Id { get; set; }
    }
}