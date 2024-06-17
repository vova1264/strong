using System.ComponentModel.DataAnnotations;

namespace StrongSite.Models
{
    public class Requests
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
    }
}