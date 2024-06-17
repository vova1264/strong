using System.Collections.Generic;
using System.Data.Entity;

using StrongSite.Operations;

namespace StrongSite.Models
{
    public class ModelContext : DbContext
    {
        public ModelContext() : base("StrongSiteConnection") { }
        public PageViewModel PageViewModel { get; set; }
        public IEnumerable<User> UserPage { get; set; }
        public IEnumerable<References> ReferencePage { get; set; }
        public IEnumerable<Customers> CustomerPage { get; set; }
        public IEnumerable<MenageBalanses> MenageBalansePage { get; set; }
        public DbSet<References> Reference { get; set; }
        public DbSet<Customers> Customer { get; set; }
        public DbSet<Cards> Card { get; set; }
        public DbSet<MenageBalanses> MenageBalanse { get; set; }
        public DbSet<Requests> Request { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<ModelContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
    public class UserDBInitializer : DropCreateDatabaseAlways<ModelContext>
    {
        protected override void Seed(ModelContext db)
        {
            db.Roles.Add(new Role { Id = 1, Name = "Руководитель" });
            db.Roles.Add(new Role { Id = 2, Name = "Администратор" });
            db.Roles.Add(new Role { Id = 3, Name = "Региональный менеджер" });
            db.Roles.Add(new Role { Id = 4, Name = "Менеджер" });
            db.Roles.Add(new Role { Id = 5, Name = "Инструктор" });
            db.Roles.Add(new Role { Id = 6, Name = "Стажёр" });
            db.Users.Add(new User
            {
                Id = 1,
                Email = "111@mail.ru",
                Password = "111",
                AccessLevel = "Руководитель",
                UserName = "Руководитель",
                Phone = "+71111111111",
                Bankcards = "1111-1111-1111-1111",
                Balance = 0,
                RoleId = 1
            });
            base.Seed(db);
        }
    }
}