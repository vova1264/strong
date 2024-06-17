using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using StrongSite.Models;

namespace StrongSite.Controllers
{
    public class HomeController : Controller
    {
        private ModelContext database;
        public HomeController(ModelContext _database)
        {
            database = _database;
        }
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("CustomerRegistry", "Customer");
            }
            else
            {
                return RedirectToAction("Cards", "BankCard", new { tip = "Дебетовая карта" });
            }
        }
        public ActionResult Message()
        {
            return View();
        }
        [Authorize]
        public ActionResult UserProfile()
        {
            var queryUser = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            switch (queryUser.AccessLevel)
            {
                case "Стажёр":
                    var pending = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "В ожидании").Count(); //Колличество заявок
                    var rejected = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Отклонена").Count(); //Колличество заявок
                    var apruve = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Одобрена").Count(); //Колличество заявок
                    var pymant = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Оплачено").Count(); //Колличество заявок
                    ViewBag.Pending = pending;
                    ViewBag.Rejected = rejected;
                    ViewBag.Apruve = apruve;
                    ViewBag.Pymant = pymant;

                    int num = pending + rejected + apruve + pymant;
                    ViewBag.PendingProc = Math.Round(((double)pending / num) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)rejected / num) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)apruve / num) * 100);
                    ViewBag.PymantProc = Math.Round(((double)pymant / num) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(pymant + apruve) / Convert.ToDouble(num) * 100, 1).ToString();
                    break;
                case "Инструктор":
                    var Agentpending = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "В ожидании").Count(); //Колличество заявок
                    var Agentrejected = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Отклонена").Count(); //Колличество заявок
                    var Agentapruve = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Одобрена").Count(); //Колличество заявок
                    var Agentpymant = database.Customer.Where(x => x.UserId == queryUser.Id && x.Status == "Оплачено").Count(); //Колличество заявок
                    ViewBag.Pending = Agentpending;
                    ViewBag.Rejected = Agentrejected;
                    ViewBag.Apruve = Agentapruve;
                    ViewBag.Pymant = Agentpymant;

                    int Agentnum = Agentpending + Agentrejected + Agentapruve + Agentpymant;
                    ViewBag.PendingProc = Math.Round(((double)Agentpending / Agentnum) * 100);
                    ViewBag.RejectedProc = Math.Round(((double)Agentrejected / Agentnum) * 100);
                    ViewBag.ApruveProc = Math.Round(((double)Agentapruve / Agentnum) * 100);
                    ViewBag.PymantProc = Math.Round(((double)Agentpymant / Agentnum) * 100);

                    ViewBag.Probability = Math.Round(Convert.ToDouble(Agentpymant + Agentapruve) / Convert.ToDouble(Agentnum) * 100, 1).ToString();
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
            return View(queryUser);
        }
        public async Task<ActionResult> GetPostback(string status, int aff_sub1)
        {
            Customers customersQuery = database.Customer.FirstOrDefault(x => x.Id == aff_sub1);
            var userNameQuery = database.Users.FirstOrDefault(d => d.Id == customersQuery.UserId);
            if (status != "pending")
            {
                User UserQuery = database.Users.Find(userNameQuery.Id);
                if (status == "approved" && customersQuery.DateApproval == null)
                {
                    customersQuery.Status = "Одобрена";
                    customersQuery.DelitedCheck = false;
                    customersQuery.DateApproval = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                    database.Entry(customersQuery).State = EntityState.Modified;
                    await database.SaveChangesAsync();
                }
                else if (status == "rejected" && customersQuery.DateApproval != null)
                {
                    customersQuery.Status = "Списана";
                    customersQuery.DateApproval = null;
                    database.Entry(customersQuery).State = EntityState.Modified;
                    await database.SaveChangesAsync();
                }
                else if (status == "rejected" && customersQuery.DateApproval == null)
                {
                    customersQuery.Status = "Отклонена";
                    database.Entry(customersQuery).State = EntityState.Modified;
                    await database.SaveChangesAsync();
                }
            }
            return RedirectToAction("Login", "Account");
        }
        public ActionResult ChangeTheme()
        {
            HttpCookie cookie = new HttpCookie("theme");
            // Добавить куки в ответ
            Response.Cookies.Add(cookie);

            if (Request.Cookies["theme"].Value == null)
            {
                Response.Cookies["theme"].Value = "dark";
            }
            else
            {
                if (Request.Cookies["theme"].Value == "dark")
                {
                    Response.Cookies["theme"].Value = "light";
                }
                else if (Request.Cookies["theme"].Value == "light")
                {
                    Response.Cookies["theme"].Value = "dark";
                }
            }

            return RedirectToAction("UserProfile");
        }
    }
}