using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domovoy_DataAccess;
using Domovoy_Models;
using System.Collections.Generic;
using Domovoy_Utility;
using Domovoy_DataAccess.Repository.IRepository;

namespace Domovoy.Controllers
{
    
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;

        public InquiryController(IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo)
        {
            _inqHRepo = inqHRepo;
            _inqDRepo = inqDRepo;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
