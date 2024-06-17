using System.ComponentModel.DataAnnotations;

namespace StrongSite.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccessLevel { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Bankcards { get; set; }
        public string Status { get; set; }
        public int Balance { get; set; }
        public int RoleId { get; set; }
        public Role Roles_Id { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}