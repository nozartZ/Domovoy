using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domovoy_DataAccess;
using Domovoy_Models;
using System.Collections.Generic;
using Domovoy_Utility;
using Domovoy_DataAccess.Repository.IRepository;
using Domovoy_Models.ViewModels;

namespace Domovoy.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        
        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController(IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo)
        {
            _inqHRepo = inqHRepo;
            _inqDRepo = inqDRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {

            InquiryVM = new InquiryVM()
            {
                inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == id),
                inquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == id,includeProperties:"Product")
            };    
            return View(InquiryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            InquiryVM.inquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.inquiryHeader.Id);

            foreach(var detail in InquiryVM.inquiryDetail)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = detail.ProductId,
                    SqFt = 1
                };
                shoppingCartList.Add(shoppingCart);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            HttpContext.Session.Set(WC.SessionInquiryId, InquiryVM.inquiryHeader.Id);
            return RedirectToAction("Index","Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == InquiryVM.inquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetails = _inqDRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.inquiryHeader.Id);
            _inqDRepo.RemoveRange(inquiryDetails);
            _inqHRepo.Remove(inquiryHeader);
            _inqHRepo.Save();
            TempData[WC.Success] = "Действие выполнено успешно";
            return RedirectToAction (nameof(Index));

        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inqHRepo.GetAll() });
        }



        #endregion
    }
}
