using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Broker.Operations;

using StrongSite.Models;
using StrongSite.Operations;

namespace StrongSite.Controllers
{
    public class AdminController : Controller
    {
        private ModelContext database;
        public AdminController(ModelContext _database)
        {
            database = _database;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> ListOfEmployees(int page = 1, string emailList = "", string accessLevelList = "",
            string userNameList = "", string phoneList = "")
        {
            ViewBag.Title = "Сотрудники";
            IQueryable<User> source;
            var queryUser = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            var dadas = database.Users.Where(u => u.Email == emailList).ToList();
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Руководитель") || User.IsInRole("Администратор"))
                {
                    source = database.Users.OrderByDescending(x => x.Id);
                }
                else if (User.IsInRole("Менеджер"))
                {
                    var queryManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    var userAgent = database.Users.Where(x => queryManage.Any(y => y == x.UserId) && x.AccessLevel == "Инструктор").Select(x => x.Id);
                    source = database.Users.Where(x => x.RoleId >= 3 && (x.Id == queryUser.Id || x.UserId == queryUser.Id || queryManage.Any(y => y == x.UserId) || userAgent.Any(y => y == x.UserId))).OrderByDescending(x => x.Id);
                }
                else if (User.IsInRole("Региональный менеджер"))
                {
                    var queryManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    source = database.Users.Where(x => x.RoleId >= 4 && (x.Id == queryUser.Id || x.UserId == queryUser.Id || queryManage.Any(y => y == x.UserId))).OrderByDescending(x => x.Id);
                }
                else if (User.IsInRole("Инструктор"))
                {
                    var queryManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    source = database.Users.Where(x => x.RoleId >= 5 && (x.Id == queryUser.Id || x.UserId == queryUser.Id || queryManage.Any(y => y == x.UserId))).OrderByDescending(x => x.Id);
                }
                else
                {
                    return View("Не удалось найти зависимости для текущего пользователя");
                }

                if (!String.IsNullOrEmpty(emailList))
                {
                    source = source.Where(p => p.Email.Contains(emailList));
                    ViewBag.emailList = emailList;
                }
                if (!String.IsNullOrEmpty(accessLevelList))
                {
                    source = source.Where(p => p.AccessLevel.Contains(accessLevelList));
                    ViewBag.accessLevelList = accessLevelList;
                }
                if (!String.IsNullOrEmpty(userNameList))
                {
                    source = source.Where(p => p.UserName.Contains(userNameList));
                    ViewBag.userNameList = userNameList;
                }
                if (!String.IsNullOrEmpty(phoneList))
                {
                    source = source.Where(p => p.Phone.Contains(phoneList));
                    ViewBag.phoneList = phoneList;
                }

                int pageSize = 500;   // количество элементов на странице

                var count = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

                ModelContext viewModel = new ModelContext
                {
                    PageViewModel = pageViewModel,
                    UserPage = items
                };
                return View(viewModel);
            }
            else
            {
                return View("Не удалось найти зависимости");
            }
        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ListOfEmployeesEdit")]
        public ActionResult ListOfEmployeesEdit(int[] arrayId)
        {
            int dd = 0;
            if (arrayId.Count() == 1)
            {
                foreach (var id in arrayId)
                {
                    dd = id;
                }
                return RedirectToAction("ListOfEmployeesEdit/" + dd);
            }
            else
            {
                return View("Выбранно больше 1 элемента");
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult ListOfEmployeesEdit(int id)
        {
            ViewBag.Title = "Профиль";
            var queryUser = database.Users.FirstOrDefault(d => d.Id == id);
            ViewBag.AccessLevel = queryUser.AccessLevel;
            switch (queryUser.AccessLevel)
            {
                case "Стажёр":
                    ViewBag.UserName = queryUser.UserName;
                    int pending = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "В ожидании").Count(); //Колличество заявок
                    int rejected = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Отклонена").Count(); //Колличество заявок
                    int apruve = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Одобрена").Count(); //Колличество заявок
                    int pymant = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Оплачено").Count(); //Колличество заявок
                    ViewBag.Pending = pending;
                    ViewBag.Rejected = rejected;
                    ViewBag.Apruve = apruve;
                    ViewBag.Pymant = pymant;

                    int num = pending + rejected + apruve + pymant;
                    ViewBag.PendingProc = Math.Round(((double)pending / num) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)rejected / num) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)apruve / num) * 100);
                    ViewBag.PymantProc = Math.Round(((double)pymant / num) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(pymant + apruve) / Convert.ToDouble(num) * 100, 1);
                    break;
                case "Инструктор":
                    ViewBag.UserName = queryUser.UserName;
                    int Agentpending = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "В ожидании").Count(); //Колличество заявок
                    int Agentrejected = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Отклонена").Count(); //Колличество заявок
                    int Agentapruve = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Одобрена").Count(); //Колличество заявок
                    int Agentpymant = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Оплачено").Count(); //Колличество заявок
                    ViewBag.Pending = Agentpending;
                    ViewBag.Rejected = Agentrejected;
                    ViewBag.Apruve = Agentapruve;
                    ViewBag.Pymant = Agentpymant;

                    int Agentnum = Agentpending + Agentrejected + Agentapruve + Agentpymant;
                    ViewBag.PendingProc = Math.Round(((double)Agentpending / Agentnum) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)Agentrejected / Agentnum) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)Agentapruve / Agentnum) * 100);
                    ViewBag.PymantProc = Math.Round(((double)Agentpymant / Agentnum) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(Agentpymant + Agentapruve) / Convert.ToDouble(Agentnum) * 100, 1);
                    break;
                case "Менеджер":
                    var queryUserRegionManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    var queryUserManageInstructor = database.Users.Where(x => queryUserRegionManage.Any(y => y == x.UserId)).Select(x => x.Id);
                    var queryUserManager = database.Users.Where(x => queryUserManageInstructor.Any(y => y == x.UserId)).Select(x => x.Id);
                    int ManagerPending = database.Customer.Where(x => x.Status == "В ожидании" && (x.UserId == queryUser.Id || queryUserRegionManage.Any(y => y == x.UserId) || queryUserManageInstructor.Any(y => y == x.UserId) || queryUserManager.Any(y => y == x.UserId))).Count();
                    int ManagerRejected = database.Customer.Where(x => x.Status == "Отклонена" && (x.UserId == queryUser.Id || queryUserRegionManage.Any(y => y == x.UserId) || queryUserManageInstructor.Any(y => y == x.UserId) || queryUserManager.Any(y => y == x.UserId))).Count();
                    int ManagerApruve = database.Customer.Where(x => x.Status == "Одобрена" && (x.UserId == queryUser.Id || queryUserRegionManage.Any(y => y == x.UserId) || queryUserManageInstructor.Any(y => y == x.UserId) || queryUserManager.Any(y => y == x.UserId))).Count();
                    int ManagerPymant = database.Customer.Where(x => x.Status == "Оплачено" && (x.UserId == queryUser.Id || queryUserRegionManage.Any(y => y == x.UserId) || queryUserManageInstructor.Any(y => y == x.UserId) || queryUserManager.Any(y => y == x.UserId))).Count();
                    ViewBag.Pending = ManagerPending;
                    ViewBag.Rejected = ManagerRejected;
                    ViewBag.Apruve = ManagerApruve;
                    ViewBag.Pymant = ManagerPymant;

                    int Managernum = ManagerPending + ManagerRejected + ManagerApruve + ManagerPymant;
                    ViewBag.PendingProc = Math.Round(((double)ManagerPending / Managernum) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)ManagerRejected / Managernum) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)ManagerApruve / Managernum) * 100);
                    ViewBag.PymantProc = Math.Round(((double)ManagerPymant / Managernum) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(ManagerPymant + ManagerApruve) / Convert.ToDouble(Managernum) * 100, 1);
                    break;
                case "Региональный менеджер":
                    var queryUserManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    var queryUserInstructor = database.Users.Where(x => queryUserManage.Any(y => y == x.UserId)).Select(x => x.Id);
                    int Managepending = database.Customer.Where(x => x.Status == "В ожидании" && (x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId) || queryUserInstructor.Any(y => y == x.UserId))).Count();
                    int Managerejected = database.Customer.Where(x => x.Status == "Отклонена" && (x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId) || queryUserInstructor.Any(y => y == x.UserId))).Count();
                    int Manageapruve = database.Customer.Where(x => x.Status == "Одобрена" && (x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId) || queryUserInstructor.Any(y => y == x.UserId))).Count();
                    int Managepymant = database.Customer.Where(x => x.Status == "Оплачено" && (x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId) || queryUserInstructor.Any(y => y == x.UserId))).Count();
                    ViewBag.Pending = Managepending;
                    ViewBag.Rejected = Managerejected;
                    ViewBag.Apruve = Manageapruve;
                    ViewBag.Pymant = Managepymant;

                    int Managnum = Managepending + Managerejected + Manageapruve + Managepymant;
                    ViewBag.PendingProc = Math.Round(((double)Managepending / Managnum) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)Managerejected / Managnum) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)Manageapruve / Managnum) * 100);
                    ViewBag.PymantProc = Math.Round(((double)Managepymant / Managnum) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(Managepymant + Manageapruve) / Convert.ToDouble(Managnum) * 100, 1);
                    break;
                case "Руководитель":

                    break;
            }
            if (queryUser == null)
            {
                return View("Не найдено");
            }
            return View(queryUser);
        }
        [Authorize]
        [HttpPost]
        public ActionResult ListOfEmployeesSave(User user)
        {
            if (user.AccessLevel == "Инструктор")
            {
                var queryAgentId = database.Users.FirstOrDefault(x => x.Id == user.UserId);
                user.UserId = queryAgentId.Id;
                user.RoleId = 5;
            }
            database.Users.Add(user);
            database.Entry(user).State = EntityState.Modified;
            database.SaveChanges();
            return RedirectToAction("ListOfEmployees");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteEmployees")]
        public ActionResult DelitedCheckbox(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    GeneralOperations.Delete<User>(database, idCheck);
                }
                database.SaveChanges();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("ListOfEmployees");
        }
        [Authorize(Roles = "Руководитель")]
        public ActionResult Requests()
        {
            ViewBag.Title = "Заявки на регистрацию";
            return View(database.Request);
        }
        [HttpPost]
        public ActionResult RequestsMessage(Requests rec)
        {
            database.Request.Add(rec);
            database.SaveChanges();
            return RedirectToAction("Message", "Home");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost, ActionName("DeleteRequests")]
        public ActionResult DeleteRequests(int id)
        {
            database.Request.Remove(database.Request.Find(id));
            database.SaveChanges();
            return RedirectToAction("Requests");
        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ListOfEmployeesUpdate")]
        public async Task<ActionResult> ListOfEmployeesUpdate()
        {
            foreach (User entity in database.Users.ToList())
            {
                int num = 0;
                foreach (Customers customer in database.Customer.ToList())
                {
                    if (entity.Id == customer.UserId && customer.User_Id.AccessLevel == "Инструктор")
                    {
                        if (customer.Status == "Одобрена")
                        {
                            num += customer.Payment;
                        }
                    }
                    else if (entity.Id == customer.UserId && customer.User_Id.AccessLevel == "Стажёр")
                    {
                        if (customer.Status == "Одобрена")
                        {
                            num += customer.Payment;
                        }
                    }
                }
                if (entity.AccessLevel == "Региональный менеджер")
                {
                    var usersAent = database.Users.Where(x => x.UserId == entity.Id && x.AccessLevel == "Инструктор").Select(x => x.Id);
                    var usersAent2 = database.Users.Where(x => x.UserId == entity.Id && x.AccessLevel == "Стажёр").Select(x => x.Id);
                    var usersAent3 = database.Users.Where(x => usersAent.Any(y => y == x.UserId) && x.AccessLevel == "Стажёр").Select(x => x.Id);
                    num = database.Customer.Where(x => (usersAent3.Any(y => y == x.UserId) || usersAent.Any(y => y == x.UserId) || usersAent2.Any(y => y == x.UserId)) && x.Status == "Одобрена").Sum(x => (int?)x.PaymentMenage) ?? 0;
                    num += database.Customer.Where(x => x.UserId == entity.Id && x.Status == "Одобрена").Sum(x => (int?)x.PaymentMenage + (int?)x.Payment) ?? 0;
                }
                if (entity.AccessLevel == "Менеджер")
                {
                    var usersMenage = database.Users.Where(x => x.UserId == entity.Id && x.AccessLevel == "Региональный менеджер").Select(x => x.Id);
                    var usersAent = database.Users.Where(x => usersMenage.Any(y => y == x.UserId) && x.AccessLevel == "Инструктор").Select(x => x.Id);
                    var usersAent2 = database.Users.Where(x => usersMenage.Any(y => y == x.UserId) && x.AccessLevel == "Стажёр").Select(x => x.Id);
                    var usersAent3 = database.Users.Where(x => usersAent.Any(y => y == x.UserId) && x.AccessLevel == "Стажёр").Select(x => x.Id);
                    num = database.Customer.Where(x => (usersMenage.Any(y => y == x.UserId) || usersAent3.Any(y => y == x.UserId) || usersAent.Any(y => y == x.UserId) || usersAent2.Any(y => y == x.UserId)) && x.Status == "Одобрена").Sum(x => (int?)x.PaymentMargin) ?? 0;
                }
                entity.Balance = num;
                database.Users.Add(entity);
                database.Entry(entity).State = EntityState.Modified;
            }
            await database.SaveChangesAsync();
            return RedirectToAction("ListOfEmployees");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AllPaymentCustomerRegistry")]
        public async Task<ActionResult> AllPaymentCustomerRegistry()
        {
            foreach (var it in database.Customer.ToList())
            {
                if (it.Status == "Одобрена")
                {
                    Customers customer = await database.Customer.FindAsync(it.Id);
                    customer.Status = "Оплачено";
                    database.Customer.Add(customer);
                    database.Entry(customer).State = EntityState.Modified;
                    await database.SaveChangesAsync();
                }
            }
            return RedirectToAction("ListOfEmployees");
        }
    }
}