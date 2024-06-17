using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

using Microsoft.Ajax.Utilities;

using StrongSite.Models;
using StrongSite.Operations;

namespace StrongSite.Controllers
{
    public class BankCardController : Controller
    {
        private ModelContext database;
        public BankCardController(ModelContext _database)
        {
            database = _database;
        }

        public ActionResult Cards(string tip)
        {
            ViewBag.Title = tip;
            if (!Request.IsAuthenticated)
            {
                var queryUser = database.Users.FirstOrDefault(x => x.AccessLevel == "Руководитель");
                var queryReference = database.Reference.Where(x => x.UserId == queryUser.Id).Select(x => x.CardId);
                var querycardAll = database.MenageBalanse.Where(x => x.Cards_Id.DelitedCheck == false && queryReference.Any(y => y == x.Cards_Id.Id) && x.Cards_Id.ProductType == tip).Include(c => c.Cards_Id);
                return View(querycardAll.DistinctBy(r => r.Cards_Id.Title));
            }
            else if (User.IsInRole("Руководитель") || User.IsInRole("Администратор") || User.IsInRole("Менеджер"))
            {
                return RedirectToAction("CardsAdmin", new { tip = tip });
            }
            else
            {
                var queryUser = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                User queryMetaId = null;
                if (User.IsInRole("Инструктор"))
                {
                    queryMetaId = database.Users.FirstOrDefault(x => x.Id == queryUser.UserId);
                }
                else if (User.IsInRole("Стажёр"))
                {
                    var qureyUser = database.Users.FirstOrDefault(x => x.Id == queryUser.UserId);
                    if (qureyUser.AccessLevel == "Инструктор")
                    {
                        queryMetaId = database.Users.FirstOrDefault(x => x.Id == qureyUser.UserId);
                    }
                    else
                    {
                        queryMetaId = qureyUser;
                    }
                }
                else if (User.IsInRole("Региональный менеджер"))
                {
                    queryMetaId = queryUser;
                }
                var queryReference = database.Reference.Where(x => x.UserId == queryMetaId.Id).Select(x => x.CardId);
                var querycard = database.MenageBalanse.Where(x => x.Cards_Id.DelitedCheck == false && queryReference.Any(y => y == x.Cards_Id.Id) && x.Cards_Id.ProductType == tip && x.User_Id.Id == queryMetaId.Id).Include(c => c.Cards_Id);
                if (querycard != null)
                {
                    return View(querycard.DistinctBy(r => r.Cards_Id.Title));
                }
                else
                {
                    return View("Здесь пока ничего нет");
                }
            }
        }
        [Authorize(Roles = "Руководитель, Менеджер")]
        public ActionResult CardsAdmin(string tip)
        {
            ViewBag.Title = tip;
            var querycardAll = database.Card.Where(x => x.ProductType == tip);
            return View(querycardAll.DistinctBy(r => r.Title));
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        public async Task<ActionResult> CardsAdd(Cards card, HttpPostedFileBase uploads)
        {
            if (uploads != null)
            {
                var fileName = Guid.NewGuid().ToString() +
                Path.GetExtension(uploads.FileName);
                var uploadUrl = Server.MapPath("~/Resources/CardsImage");
                uploads.SaveAs(Path.Combine(uploadUrl, fileName));
                card.Image = fileName;
            }
            card.DelitedCheck = false;
            database.Card.Add(card);
            await database.SaveChangesAsync();
            return RedirectToAction("Cards", "BankCard", new { tip = card.ProductType });
        }
        [Authorize(Roles = "Руководитель")]
        [HttpGet, ActionName("DeleteCards")]
        public ActionResult DeleteCards(int id)
        {
            var сardsIdentity = database.Card.Find(id);
            сardsIdentity.DelitedCheck = true;
            database.Card.Add(сardsIdentity);
            database.Entry(сardsIdentity).State = EntityState.Modified;
            database.SaveChangesAsync();
            return RedirectToAction("Cards", new { tip = сardsIdentity.ProductType });
        }
        [Authorize(Roles = "Руководитель")]
        [HttpGet, ActionName("RecoverCards")]
        public ActionResult RecoverCards(int id)
        {
            var сardsIdentity = database.Card.Find(id);
            сardsIdentity.DelitedCheck = false;
            database.Card.Add(сardsIdentity);
            database.Entry(сardsIdentity).State = EntityState.Modified;
            database.SaveChangesAsync();
            return RedirectToAction("Cards", new { tip = сardsIdentity.ProductType });
        }
        [Authorize(Roles = "Руководитель")]
        [HttpGet]
        public ActionResult CardsEdit(int id)
        {
            ViewBag.Title = "Изменение продукта";
            var course = database.Card.FirstOrDefault(d => d.Id == id);
            if (course == null)
            {
                return View("Не найдено");
            }
            ViewBag.CardName = course.Title;
            return View(course);
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        public async Task<ActionResult> CardsSave(Cards card, HttpPostedFileBase uploads)
        {
            if (uploads != null)
            {
                var fileName = Guid.NewGuid().ToString() +
                Path.GetExtension(uploads.FileName);
                var uploadUrl = Server.MapPath("~/Resources/CardsImage");
                uploads.SaveAs(Path.Combine(uploadUrl, fileName));
                card.Image = fileName;
            }
            database.Card.Add(card);
            database.Entry(card).State = EntityState.Modified;
            await database.SaveChangesAsync();
            return RedirectToAction("Cards", new { tip = card.ProductType });
        }
        [Authorize(Roles = "Руководитель")]
        [HttpGet]
        public ActionResult AmountRegistryAdd(int id)
        {
            ViewBag.Title = "Реестр сумм";
            var card = database.Card.FirstOrDefault(d => d.Id == id);
            if (card == null)
            {
                return View("Не найдено");
            }
            if (User.IsInRole("Руководитель"))
            {
                ViewBag.UserName = new SelectList(database.Users.Where(x => x.AccessLevel == "Региональный менеджер").ToList(), "Email", "UserName");
            }
            else
            {
                var userId = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                var query = database.Users.Where(x => x.UserId == userId.Id).Select(x => x.Id);
                ViewBag.UserName = new SelectList(database.Users.Where(x => query.Any(y => y == x.Id) && x.AccessLevel == "Региональный менеджер").ToList(), "Email", "UserName");
            }
            return View(card);
        }
        [Authorize(Roles = "Руководитель")]
        [HttpPost]
        public async Task<ActionResult> AmountRegistrySave(Cards card, string UserName, int maxAmount, int amount, int marginAmount)
        {
            var Name = UserName.IsEmpty() ? User.Identity.Name : UserName;
            var queryUser = database.Users.FirstOrDefault(x => x.Email == Name);
            var menage = database.MenageBalanse.FirstOrDefault(x => x.UserId == queryUser.Id && x.CardsId == card.Id);
            MenageBalanses menageBalanses = new MenageBalanses();
            if (menage != null)
            {
                menageBalanses = database.MenageBalanse.FirstOrDefault(x => x.Id == menage.Id);
            }
            if (menage == null)
            {
                menageBalanses.Amount = amount;
                menageBalanses.MaxAmount = maxAmount;
                menageBalanses.MarginAmount = marginAmount;
                menageBalanses.UserId = queryUser.Id;
                menageBalanses.CardsId = card.Id;
                database.MenageBalanse.Add(menageBalanses);
                await database.SaveChangesAsync();
            }
            else
            {
                menageBalanses.Amount = amount;
                menageBalanses.MaxAmount = maxAmount;
                menageBalanses.MarginAmount = marginAmount;
                menageBalanses.UserId = menage.UserId;
                menageBalanses.CardsId = card.Id;
                database.MenageBalanse.Add(menageBalanses);
                database.Entry(menageBalanses).State = EntityState.Modified;
                await database.SaveChangesAsync();
            }
            return RedirectToAction("AmountRegistry", "BankCard");
        }
        [Authorize(Roles = "Руководитель, Менеджер, Региональный менеджер")]
        [HttpGet]
        public async Task<ActionResult> AmountRegistry(int page = 1,
            string produktList = "", string emailList = "", string userList = "")
        {
            ViewBag.Title = "Реестр цен";
            IQueryable<MenageBalanses> source = database.MenageBalanse.Include(c => c.Cards_Id).Include(c => c.User_Id);
            var queryUser = database.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Руководитель") || User.IsInRole("Администратор"))
                {
                    source = source.OrderBy(x => x.Id);
                }
                else if (User.IsInRole("Менеджер"))
                {
                    var queryManage = database.Users.Where(x => x.UserId == queryUser.Id).Select(x => x.Id);
                    source = source.Where(x => queryManage.Any(y => y == x.UserId)).OrderBy(x => x.Id);
                }
                else if (User.IsInRole("Региональный менеджер"))
                {
                    source = source.Where(x => x.UserId == queryUser.Id).OrderBy(x => x.Id);
                }
                else
                {
                    return View("Не удалось найти зависимости для текущего пользователя");
                }
                if (!String.IsNullOrEmpty(produktList))
                {
                    source = source.Where(p => p.Cards_Id.Title.ToString().Contains(produktList));
                    ViewBag.produktList = produktList;
                }
                if (!String.IsNullOrEmpty(emailList))
                {
                    source = source.Where(p => p.User_Id.Email.Contains(emailList));
                    ViewBag.emailList = emailList;
                }
                if (!String.IsNullOrEmpty(userList))
                {
                    source = source.Where(p => p.User_Id.UserName.Contains(userList));
                    ViewBag.userList = userList;
                }
                int pageSize = 500;   // количество элементов на странице

                var count = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

                ModelContext viewModel = new ModelContext
                {
                    PageViewModel = pageViewModel,
                    MenageBalansePage = items
                };
                return View(viewModel);
            }
            else
            {
                return View("Не удалось найти зависимости");
            }
        }
        [Authorize(Roles = "Руководитель")]
        public ActionResult AmountRegistryDelete(int id)
        {
            var сardsIdentity = database.MenageBalanse.Find(id);
            database.MenageBalanse.Remove(сardsIdentity);
            database.SaveChangesAsync();
            return RedirectToAction("AmountRegistry", "BankCard");
        }
    }
}