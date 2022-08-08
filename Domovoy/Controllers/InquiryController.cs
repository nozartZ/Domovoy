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

        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inqHRepo.GetAll() });
        }



        #endregion
    }
}
