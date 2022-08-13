﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Domovoy_DataAccess;
using Domovoy_Models;
using Domovoy_Models.ViewModels;
using Domovoy_Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domovoy_DataAccess.Repository.IRepository;
using System;

namespace Domovoy.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IProductRepository prodRepo, IApplicationUserRepository userRepo, IWebHostEnvironment webHostEnvironment, 
            IEmailSender emailSender, IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
            IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo)
        {
            _prodRepo = prodRepo;
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _inqHRepo= inqHRepo;
            _inqDRepo= inqDRepo;
            _orderHRepo = orderHRepo;
            _orderDRepo = orderDRepo;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            if(HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart)!= null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }
            List <int> prodInCart = shoppingCartsList.Select(i=>i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartsList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u=>u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName ("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            ApplicationUser applicationUser;

            if(User.IsInRole(WC.AdminRole))
            {
                if(HttpContext.Session.Get<int>(WC.SessionInquiryId)!=0)
                {
                    //cart has been loaded using inquiry
                    InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == (HttpContext.Session.Get<int>(WC.SessionInquiryId)));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber

                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //var userId = User.FindFirstValue(ClaimTypes.Name);
                applicationUser = _userRepo.FirstOrDefault(u=>u.Id == claim.Value);
            };

            
           
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }
            List<int> prodInCart = shoppingCartsList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
                
            };

            foreach (var cartObj in shoppingCartsList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName ("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            var claimsIdentity= (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(User.IsInRole(WC.AdminRole))
            {
                //we need to create an order
                var orderTotal = 0.0;
                foreach(Product prod in ProductUserVM.ProductList)
                {
                    orderTotal += prod.Price * prod.TempSqFt;
                }
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = orderTotal,
                    City = ProductUserVM.ApplicationUser.City,
                    StreetAddress = ProductUserVM.ApplicationUser.StreetAdress,
                    State = ProductUserVM.ApplicationUser.State,
                    PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id
                    };
                    _orderDRepo.Add(orderDetail);
                }
                _orderDRepo.Save();
                return RedirectToAction(nameof(InqueryConfirmation), new {id=orderHeader.Id});
            }
            else
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

                var subject = "New Inquiry";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }
                //Name: { 0}
                //Email: { 1}
                //Phone: { 2}
                //Products: {3}

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append($" - Name: {prod.Name} <span style = 'font-size=14px;'> (ID: {prod.Id})</span><br />");
                }
                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };
                _inqHRepo.Add(inquiryHeader);
                _inqHRepo.Save();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id
                    };
                    _inqDRepo.Add(inquiryDetail);
                }
                _inqDRepo.Save();
                TempData[WC.Success] = "Заказ успешно отправлен";
            }

            return RedirectToAction(nameof(InqueryConfirmation));
        }

        public IActionResult InqueryConfirmation ()
        {

            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            }

            shoppingCartsList.Remove(shoppingCartsList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartsList);
            TempData[WC.Success] = "Товар удален из корзины";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable <Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

    }
}
