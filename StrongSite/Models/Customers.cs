using System.ComponentModel.DataAnnotations.Schema;

namespace StrongSite.Models
{
    public class Customers
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User_Id { get; set; }
        public int CardId { get; set; }
        [ForeignKey("CardId")]
        public Cards Cards_Id { get; set; }
        public string Image { get; set; }
        public string UserName { get; set; }
        public string DateOfCreation { get; set; }
        public string DateApproval { get; set; }
        public string BirthDate { get; set; }
        public string Comment { get; set; }
        public string StatusKC { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public int PaymentMenage { get; set; }
        public int PaymentMargin { get; set; }
        public int Payment { get; set; }
        public bool ConsentToProcessing { get; set; }
        public bool DelitedCheck { get; set; }
    }
}