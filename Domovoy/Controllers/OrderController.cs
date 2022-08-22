﻿using Domovoy_DataAccess.Repository.IRepository;
using Domovoy_Models;
using Domovoy_Models.ViewModels;
using Domovoy_Utility;
using Domovoy_Utility.BrainTree;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Domovoy.Controllers
{
    public class OrderController : Controller
    {
        
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public ProductUserVM ProductUserVM { get; set; }

        public OrderController(IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo, IBrainTreeGate brain)
        {
            _orderHRepo = orderHRepo;
            _orderDRepo = orderDRepo;
            _brain = brain;
        }
        public IActionResult Index(string searchName =null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _orderHRepo.GetAll(),
                StatusList = WC.listStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            if(!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u=>u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status!= "--Order Status--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }
        
        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id),
                orderDetail = _orderDRepo.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(OrderVM); 
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.orderHeader.Id);
            orderHeader.OrderStatus = WC.StatusProcessing;
            _orderHRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.orderHeader.Id);
            orderHeader.OrderStatus = WC.StatusShipped;
            orderHeader.ShippingDate = System.DateTime.Now;
            _orderHRepo.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == OrderVM.orderHeader.Id);
            orderHeader.OrderStatus = WC.StatusProcessing;
            _orderHRepo.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
