using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using StrongSite.Models.Authentication;

using StrongSite.Models;
using System.Security.Cryptography;

namespace StrongSite.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                using (ModelContext db = new ModelContext())
                {
                    user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                }
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, true);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Пользователя с таким логином и паролем нет");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Руководитель, Региональный менеджер, Менеджер, Инструктор")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                User query = null;
                using (ModelContext db = new ModelContext())
                {
                    user = db.Users.FirstOrDefault(u => u.Email == model.Email || u.UserName == model.UserName);
                    query = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);

                    if (user == null)
                    {
                        db.Users.Add(new User
                        {
                            UserId = query.Id,
                            Email = model.Email,
                            Password = model.Password,
                            AccessLevel = User.IsInRole("Руководитель") ? model.AccessLevel : User.IsInRole("Менеджер") ?
                            "Региональный менеджер" : User.IsInRole("Региональный менеджер") ? model.AccessLevel : "Стажёр",
                            UserName = model.UserName,
                            Status = "Работает",
                            Phone = model.Phone,
                            Bankcards = model.Bankcards,
                            RoleId = model.AccessLevel == "Администратор" ? 2 : model.AccessLevel == "Менеджер" ? 3 :
                            model.AccessLevel == "Региональный менеджер" ? 4 : model.AccessLevel == "Инструктор" ? 5 : 6,
                            Balance = 0
                        });
                        db.SaveChanges();
                        ViewBag.msgRegister = "Пользователь " + model.Email + " успешно зарегистрирован";

                        if (user != null)
                        {
                            FormsAuthentication.SetAuthCookie(model.Email, true);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Пользователь с таким логином или именем уже существует");
                    }
                }
            }
            return View(model);
        }

        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}