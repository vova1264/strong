using System.ComponentModel.DataAnnotations;

namespace StrongSite.Models
{
    public class Cards
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public string BankName { get; set; }
        public string ProductType { get; set; }
        public string DescriptionOne { get; set; }
        public string DescriptionTwo { get; set; }
        public string DescriptionThree { get; set; }
        public string DescriptionFour { get; set; }
        public bool DelitedCheck { get; set; }
    }
}