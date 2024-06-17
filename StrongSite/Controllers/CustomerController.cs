using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Broker.Operations;

using StrongSite.Models;

using StrongSite.Operations;
using QRCoder;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace StrongSite.Controllers
{
    public class CustomerController : Controller
    {
        private ModelContext database;
        private Random random;
        public CustomerController(ModelContext _database, Random _random)
        {
            database = _database;
            random = _random;
        }
        [Authorize]
        public async Task<ActionResult> CustomerRegistry(int page = 1, bool allRows = false, bool? deliteCheck = null,
            string subIdList = "", string userList = "",
            string phoneList = "", string productNameList = "",
            string creatorList = "", string dateOfCreationList = "",
            string commentList = "", string statusList = "", string statusKCList = "")
        {
            ViewBag.Title = "Реестр заявок";
            IQueryable<Customers> source = database.Customer.Include(b => b.User_Id).Include(b => b.Cards_Id);
            var queryUser = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (User.IsInRole("Руководитель") || User.IsInRole("Администратор"))
            {
                source = source.OrderByDescending(x => x.Id);
            }
            else if (User.IsInRole("Менеджер"))
            {
                var queryUserRegionManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                var queryUserManage = database.Users.Where(x => queryUserRegionManage.Any(y => y == x.UserId)).Select(x => x.Id);

                var userAgent = database.Users.Where(x => queryUserManage.Any(y => y == x.UserId)).Select(x => x.Id);

                source = source.Where(x => x.UserId == queryUser.Id || queryUserRegionManage.Any(y => y == x.UserId) || queryUserManage.Any(y => y == x.UserId) || userAgent.Any(y => y == x.UserId)).OrderByDescending(x => x.Id);
            }
            else if (User.IsInRole("Региональный менеджер"))
            {
                var queryUserManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);

                var userAgent = database.Users.Where(x => queryUserManage.Any(y => y == x.UserId)).Select(x => x.Id);

                source = source.Where(x => x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId) || userAgent.Any(y => y == x.UserId)).OrderByDescending(x => x.Id);
            }
            else if (User.IsInRole("Инструктор"))
            {
                var queryUserManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                source = source.Where(x => x.UserId == queryUser.Id || queryUserManage.Any(y => y == x.UserId)).OrderByDescending(x => x.Id);

            }
            else if (User.IsInRole("Стажёр"))
            {
                source = source.Where(x => x.UserId == queryUser.Id).OrderByDescending(x => x.Id);
            }

            if (deliteCheck == null)
            {
                source = source.Where(p => p.DelitedCheck == false);
                ViewBag.deliteCheck = deliteCheck;
            }
            else
            {
                if (deliteCheck.Value)
                {
                    source = source.Where(p => p.DelitedCheck == true);
                    ViewBag.deliteCheck = deliteCheck;
                }
                else
                {
                    ViewBag.deliteCheck = deliteCheck;
                }
            }
            if (!String.IsNullOrEmpty(subIdList))
            {
                source = source.Where(p => p.Id.ToString().Contains(subIdList));
                ViewBag.subIdList = subIdList;
            }
            if (!String.IsNullOrEmpty(userList))
            {
                source = source.Where(p => p.UserName.Contains(userList));
                ViewBag.userList = userList;
            }
            if (!String.IsNullOrEmpty(phoneList))
            {
                source = source.Where(p => p.Phone.Contains(phoneList));
                ViewBag.phoneList = phoneList;
            }
            if (!String.IsNullOrEmpty(productNameList))
            {
                source = source.Where(p => p.Cards_Id.Title.Contains(productNameList));
                ViewBag.productNameList = productNameList;
            }
            if (!String.IsNullOrEmpty(creatorList))
            {
                source = source.Where(p => p.User_Id.UserName.Contains(creatorList));
                ViewBag.creatorList = creatorList;
            }
            if (!String.IsNullOrEmpty(dateOfCreationList))
            {
                source = source.Where(p => p.DateOfCreation.Contains(dateOfCreationList));
                ViewBag.dateOfCreationList = dateOfCreationList;
            }
            if (!String.IsNullOrEmpty(commentList))
            {
                source = source.Where(p => p.Comment.Contains(commentList));
                ViewBag.commentList = commentList;
            }
            if (!String.IsNullOrEmpty(statusList))
            {
                source = source.Where(p => p.Status == statusList);
                ViewBag.statusList = statusList;
            }
            if (!String.IsNullOrEmpty(statusKCList))
            {
                source = source.Where(p => p.StatusKC == statusKCList);
                ViewBag.statusKCList = statusKCList;
            }

            int pageSize;   // количество элементов на странице
            if (!allRows)
            {
                pageSize = 500;   // количество элементов на странице
            }
            else
            {
                pageSize = 100000;
                ViewBag.allRows = true;
            }
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

            ModelContext viewModel = new ModelContext
            {
                PageViewModel = pageViewModel,
                CustomerPage = items
            };
            return View(viewModel);
        }

        public ActionResult CardsOpen(int id)
        {
            ViewBag.Title = "Создание заявки";
            ViewBag.Linktrue = true;
            if (User.IsInRole("Руководитель"))
            {
                return RedirectToAction("CardsEdit", "BankCard", new { id = id });
            }
            else
            {
                var card = database.Card.FirstOrDefault(x => x.Id == id);
                if (!Request.IsAuthenticated)
                {
                    List<int> referenceList = new List<int>();
                    var queryUser = database.Users.FirstOrDefault(x => x.RoleId == 1);
                    var queryReference = database.Reference.Where(d => d.Cards_Id.Title == card.Title && d.User_Id.Email == queryUser.Email);
                    foreach (var ss in queryReference)
                    {
                        referenceList.Add(ss.Id);
                    }
                    int num = referenceList[random.Next(referenceList.Count)];
                    var queryRandom = database.Reference.FirstOrDefault(d => d.Id == num);

                    return new RedirectResult(queryRandom.Link);
                }
                else
                {
                    return View(card);
                }
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult AddCustomers()
        {
            ViewBag.Title = "Создание заявки";
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddCustomers(Customers customer, int id)
        {
            List<int> referenceList = new List<int>();
            var card = database.Card.FirstOrDefault(x => x.Id == id);
            var user = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            User userIdentity = null;
            if (user.AccessLevel == "Стажёр")
            {
                var qureyUser = database.Users.FirstOrDefault(x => x.Id == user.UserId);
                if (qureyUser.AccessLevel == "Инструктор")
                {
                    userIdentity = database.Users.FirstOrDefault(x => x.Id == qureyUser.UserId);
                }
                else
                {
                    userIdentity = qureyUser;
                }
            }
            else if (user.AccessLevel == "Инструктор")
            {
                userIdentity = database.Users.FirstOrDefault(x => x.Id == user.UserId && x.AccessLevel == "Региональный менеджер");
            }
            else if (user.AccessLevel == "Региональный менеджер")
            {
                userIdentity = user;
            }

            var menageBalanse = database.MenageBalanse.FirstOrDefault(x => x.CardsId == card.Id && x.UserId == userIdentity.Id);
            var queryReference = database.Reference.Where(d => d.Cards_Id.Title == card.Title && d.UserId == userIdentity.Id);
            foreach (var ss in queryReference)
            {
                referenceList.Add(ss.Id);
            }
            int num = referenceList[random.Next(referenceList.Count)];
            var queryRandom = database.Reference.FirstOrDefault(d => d.Id == num);
            customer.CardId = card.Id;
            customer.UserId = user.Id;
            customer.DateOfCreation = DateTime.Now.Day.ToString("00") + "." + DateTime.Now.Month.ToString("00") + "." + DateTime.Now.Year.ToString() + " " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
            customer.Status = "В ожидании";
            customer.StatusKC = "В обработке";
            customer.Comment = "Отсутствует";
            customer.ConsentToProcessing = true;
            customer.Payment = menageBalanse.Amount;
            customer.PaymentMenage = menageBalanse.MaxAmount - menageBalanse.Amount - menageBalanse.MarginAmount;
            customer.PaymentMargin = menageBalanse.MarginAmount;
            customer.DelitedCheck = false;
            database.Customer.Add(customer);
            await database.SaveChangesAsync();

            ViewBag.Linktrue = false;
            ViewBag.LinkBamk = queryRandom.Link + "&aff_sub1=" + customer.Id;
            QRCode code;
            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCode = qrGenerator.CreateQrCode(queryRandom.Link + "&aff_sub1=" + customer.Id, QRCodeGenerator.ECCLevel.Q);
                code = new QRCode(qrCode);
                using (Bitmap bitMap = code.GetGraphic(20))
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "EditCustomerRegistry")]
        public ActionResult EditCustomerRegistry(int[] arrayId)
        {
            int dd = 0;
            if (arrayId.Count() == 1)
            {
                foreach (var id in arrayId)
                {
                    dd = id;
                }
                return RedirectToAction("EditCustomerRegistry/" + dd);
            }
            else
            {
                return View("Выбранно больше  или меньше 1 элемента");
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult EditCustomerRegistry(int id)
        {
            ViewBag.Title = "Редактирование заявки";
            var query = database.Customer.FirstOrDefault(d => d.Id == id);
            if (query == null)
            {
                return View("Не найдено");
            }
            else
            {
                return View(query);
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CustomerRegistrySave(Customers customer, HttpPostedFileBase uploads, DateTime? commentaryi)
        {
            if (uploads != null)
            {
                var fileName = Guid.NewGuid().ToString() +
                Path.GetExtension(uploads.FileName);
                var uploadUrl = Server.MapPath("~/Resources/CustomerImage");
                uploads.SaveAs(Path.Combine(uploadUrl, fileName));
                customer.Image = fileName;
            }
            if (commentaryi != null)
            {
                customer.Comment = commentaryi.Value.Day.ToString("00") + "." + commentaryi.Value.Month.ToString("00") + "." + commentaryi.Value.Year.ToString();
            }
            database.Customer.Add(customer);
            database.Entry(customer).State = EntityState.Modified;
            await database.SaveChangesAsync();
            return RedirectToAction("CustomerRegistry");

        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteCustomerRegistry")]
        public ActionResult CustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    GeneralOperations.Delete<Customers>(database, idCheck);
                }
                database.SaveChanges();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CheckCustomerRegistry")]
        public ActionResult CheckCustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    Customers customer = database.Customer.Find(idCheck);
                    customer.DelitedCheck = true;
                    database.Customer.Add(customer);
                    database.Entry(customer).State = EntityState.Modified;
                }
                database.SaveChanges();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
        [Authorize]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CheckRowsCustomerRegistry")]
        public ActionResult CheckRowsCustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    Customers customer = database.Customer.Find(idCheck);
                    customer.DelitedCheck = false;
                    database.Customer.Add(customer);
                    database.Entry(customer).State = EntityState.Modified;
                }
                database.SaveChanges();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ApprovedCustomerRegistry")]
        public async Task<ActionResult> ApprovedCustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    Customers customer = database.Customer.Find(idCheck);
                    if (customer.Status != "Одобрена")
                    {
                        var uses = database.Users.FirstOrDefault(d => d.Id == customer.UserId);
                        customer.Status = "Одобрена";
                        customer.DateApproval = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                        uses.Balance = uses.Balance + customer.Payment;
                        database.Customer.Add(customer);
                        database.Entry(customer).State = EntityState.Modified;
                    }
                }
                await database.SaveChangesAsync();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RejectedCustomerRegistry")]
        public async Task<ActionResult> RejectedCustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    Customers customer = database.Customer.Find(idCheck);
                    customer.Status = "Отклонена";
                    database.Customer.Add(customer);
                    database.Entry(customer).State = EntityState.Modified;
                }
                await database.SaveChangesAsync();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PaymentCustomerRegistry")]
        public async Task<ActionResult> PaymentCustomerRegistry(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    Customers customer = database.Customer.Find(idCheck);
                    if (customer.Status != "Оплачено")
                    {
                        var uses = database.Users.FirstOrDefault(d => d.Id == customer.UserId);
                        customer.Status = "Оплачено";
                        database.Customer.Add(customer);
                        database.Entry(customer).State = EntityState.Modified;
                        await database.SaveChangesAsync();
                    }
                }
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("CustomerRegistry");
        }
    }
}