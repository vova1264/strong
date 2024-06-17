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
    public class ReferenceCardController : Controller
    {
        private ModelContext database;
        public ReferenceCardController(ModelContext _database)
        {
            database = _database;
        }
        [HttpGet]
        public async Task<ActionResult> Reference(int page = 1, string referenceList = "", string userList = "", 
            string idPlatformList = "", string bankList = "", string productTypeList = "")
        {
            ViewBag.Title = "Реестр ссылок";
            IQueryable<References> source = database.Reference.Include(b => b.User_Id).Include(b => b.Cards_Id).OrderByDescending(x => x.IdPlatform);
            if (!String.IsNullOrEmpty(referenceList))
            {
                source = source.Where(b => b.Cards_Id.Title.Contains(referenceList));
                ViewBag.referenceList = referenceList;
            }
            if (!String.IsNullOrEmpty(userList))
            {
                source = source.Where(b => b.User_Id.UserName.Contains(userList));
                ViewBag.userList = userList;
            }
            if (!String.IsNullOrEmpty(idPlatformList))
            {
                source = source.Where(p => p.IdPlatform.ToString().Contains(idPlatformList));
                ViewBag.idPlatformList = idPlatformList;
            }
            if (!String.IsNullOrEmpty(bankList))
            {
                source = source.Where(p => p.Cards_Id.BankName.ToString().Contains(bankList));
                ViewBag.bankList = bankList;
            }
            if (!String.IsNullOrEmpty(productTypeList))
            {
                source = source.Where(p => p.Cards_Id.ProductType.ToString().Contains(productTypeList));
                ViewBag.productTypeList = productTypeList;
            }

            int pageSize = 500;   // количество элементов на странице

            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

            ModelContext viewModel = new ModelContext
            {
                PageViewModel = pageViewModel,
                ReferencePage = items
            };
            return View(viewModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteReference")]
        public ActionResult Reference(int[] arrayId)
        {
            if (arrayId != null && arrayId.Length > 0)
            {
                foreach (var idCheck in arrayId)
                {
                    GeneralOperations.Delete<References>(database, idCheck);
                }
                database.SaveChanges();
            }
            else
            {
                return View("Не выбрано ни одного элемента");
            }
            return RedirectToAction("Reference");
        }
        public ActionResult ReferenceAdd()
        {
            ViewBag.CardName1 = new SelectList(database.Card.Where(x => x.ProductType == "Дебетовая карта").ToList(), "Title", "Title");
            ViewBag.CardName2 = new SelectList(database.Card.Where(x => x.ProductType == "Кредитная карта").ToList(), "Title", "Title");
            ViewBag.CardName3 = new SelectList(database.Card.Where(x => x.ProductType == "Счёт РКО").ToList(), "Title", "Title");
            ViewBag.CardName4 = new SelectList(database.Card.Where(x => x.ProductType == "Микрозайм").ToList(), "Title", "Title");
            ViewBag.CardName5 = new SelectList(database.Card.Where(x => x.ProductType == "Инвестиции").ToList(), "Title", "Title");
            ViewBag.CardName6 = new SelectList(database.Card.Where(x => x.ProductType == "Потребительский кредит").ToList(), "Title", "Title");
            ViewBag.CardName7 = new SelectList(database.Card.Where(x => x.ProductType == "Рефинансирование").ToList(), "Title", "Title");
            ViewBag.CardName8 = new SelectList(database.Card.Where(x => x.ProductType == "Автокредит").ToList(), "Title", "Title");
            ViewBag.CardName9 = new SelectList(database.Card.Where(x => x.ProductType == "Ипотека").ToList(), "Title", "Title");
            ViewBag.CardName10 = new SelectList(database.Card.Where(x => x.ProductType == "Залоговый займ").ToList(), "Title", "Title");
            ViewBag.CardName11 = new SelectList(database.Card.Where(x => x.ProductType == "Регистрация ИП, ООО").ToList(), "Title", "Title");
            ViewBag.CardName12 = new SelectList(database.Card.Where(x => x.ProductType == "Страхование").ToList(), "Title", "Title");
            ViewBag.CardName13 = new SelectList(database.Card.Where(x => x.ProductType == "Вклады").ToList(), "Title", "Title");
            ViewBag.UserName = new SelectList(database.Users.Where(x => x.AccessLevel == "Руководитель" || x.AccessLevel == "Региональный менеджер").ToList(), "Email", "UserName");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ReferenceAdd(References reference, string UserName, string ProductType,
           string gencodx1, string gencodx2, string gencodx3, string gencodx4, string gencodx5, string gencodx6,
           string gencodx7, string gencodx8, string gencodx9, string gencodx10, string gencodx11, string gencodx12,
           string gencodx13)
        {
            var query = ProductType == "Дебетовая карта" ? gencodx1 :
                        ProductType == "Кредитная карта" ? gencodx2 :
                        ProductType == "Счёт РКО" ? gencodx3 :
                        ProductType == "Микрозайм" ? gencodx4 :
                        ProductType == "Инвестиции" ? gencodx5 :
                        ProductType == "Потребительский кредит" ? gencodx6 :
                        ProductType == "Рефинансирование" ? gencodx7 :
                        ProductType == "Автокредит" ? gencodx8 :
                        ProductType == "Ипотека" ? gencodx9 :
                        ProductType == "Залоговый займ" ? gencodx10 :
                        ProductType == "Регистрация ИП, ООО" ? gencodx11 :
                        ProductType == "Страхование" ? gencodx12 : gencodx13;
            var referenceQuerCards = database.Card.FirstOrDefault(x => x.Title == query);
            var referenceQueryUsers = database.Users.FirstOrDefault(x => x.Email == UserName);
            reference.CardId = referenceQuerCards.Id;
            reference.UserId = referenceQueryUsers.Id;
            database.Reference.Add(reference);
            await database.SaveChangesAsync();
            return RedirectToAction("Reference");
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ReferenceEdit")]
        public ActionResult ReferenceEdit(int[] arrayId)
        {
            int dd = 0;
            if (arrayId.Count() == 1)
            {
                foreach (var id in arrayId)
                {
                    dd = id;
                }
                return RedirectToAction("ReferenceEdit/" + dd);
            }
            else
            {
                return View("Выбранно больше 1 элемента");
            }
        }
        [HttpGet]
        public ActionResult ReferenceEdit(int id)
        {
            var query = database.Reference.Include(b => b.Cards_Id).FirstOrDefault(d => d.Id == id);
            if (query == null)
            {
                return View("Не найдено");
            }
            ViewBag.UserName = new SelectList(database.Users.Where(x => x.AccessLevel == "Руководитель" || x.AccessLevel == "Региональный менеджер").ToList(), "Id", "UserName");
            return View(query);
        }
        [HttpPost]
        public async Task<ActionResult> ReferenceSave(References reference, int UserId)
        {
            reference.UserId = UserId;
            database.Reference.Add(reference);
            database.Entry(reference).State = EntityState.Modified;
            await database.SaveChangesAsync();
            return RedirectToAction("Reference");
        }
    }
}