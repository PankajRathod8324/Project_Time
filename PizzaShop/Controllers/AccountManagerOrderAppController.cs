using System.Diagnostics;
using System.Security.Claims;
using DAL.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entities.ViewModel;
using X.PagedList.Extensions;
using BLL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Linq;
using DocumentFormat.OpenXml.Office.CustomUI;
using BLL.Interfaces;

namespace PizzaShop.Controllers;


public class AccountManagerOrderAppController : Controller
{
    // private readonly ILogger<HomeController> _logger;

    private readonly IAccountManagerOrderAppService _accountmanagerorderapp;
    private readonly IMenuService _menuService;
    private readonly IOrderService _orderService;

    private readonly ITaxService _taxService;
    private readonly ITableAndSectionService _tableandsectionService;


    public AccountManagerOrderAppController(IAccountManagerOrderAppService accountmanagerorderapp, ITableAndSectionService tableandsectionService, IMenuService menuService, IOrderService orderService, ITaxService taxService)
    {
        _menuService = menuService;
        _accountmanagerorderapp = accountmanagerorderapp;
        _tableandsectionService = tableandsectionService;
        _orderService = orderService;
        _taxService = taxService;
    }

    [Authorize(Policy = "AccountManagerPolicy")]
    public IActionResult Table()
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var section = _accountmanagerorderapp.GetOrderSection();
        return View(section);
    }

    [Authorize(Policy = "AccountManagerPolicy")]
    public IActionResult GetWaitingList(int sectionId)
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var waitinglist = _accountmanagerorderapp.GetAllWaitingListCustomer(sectionId);
        return PartialView("_WaitingList", waitinglist);
    }

    [Authorize(Policy = "AccountManagerPolicy")]
    public IActionResult GetWaitingData(int waitingId)
    {

        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var waitingData = _accountmanagerorderapp.GetWaitingData(waitingId);
        return PartialView("_CustomerDetailsForm", waitingData);
    }

    public IActionResult GetWaitingDataByEmail(string email)
    {

        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var waitingData = _accountmanagerorderapp.GetWaitingDataByEmail(email);
        return PartialView("_CustomerDetailsForm", waitingData);
    }

    public IActionResult AddCustomerDetail([FromBody] JsonObject customerdata)
    {
        bool success = _accountmanagerorderapp.AddOrUpdateCustomer(customerdata);
        string email = (string)customerdata["email"]!;
        var customerId = _accountmanagerorderapp.GetCustomerIDByName(email!);
        if (success)
        {
            return Json(new { success = true, message = "Customer Added Successfully!", customerId = customerId });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult GetWaitingToken(string email, int sectionId)
    {
        var sections = _tableandsectionService.GetAllSection();
        var section = _tableandsectionService.GetSectionById(sectionId);
        if (section != null)
        {
            ViewBag.Sections = sections.Where(t => t.SectionName == section.SectionName).Select(r => new SelectListItem
            {
                Value = r.SectionId.ToString(),
                Text = r.SectionName
            }).ToList();
        }
        else
        {
             ViewBag.Sections = sections.Select(r => new SelectListItem
            {
                Value = r.SectionId.ToString(),
                Text = r.SectionName
            }).ToList();
        }

        var waitingData = _accountmanagerorderapp.GetWaitingDataByEmail(email);
        return PartialView("_WaitingTokenDetailsPV", waitingData);
    }

    public IActionResult AddInWaitingList([FromBody] JsonObject customerdata)
    {
        bool success = _accountmanagerorderapp.AddInWaitingList(customerdata);

        if (success)
        {
            return Json(new { success = true, message = "Customer Added Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult GetWaitingTokenById(int waitingId)
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var waitingData = _accountmanagerorderapp.GetWaitingTokenById(waitingId);
        return Json(waitingData);
    }

    [Authorize(Policy = "ChefOrAccountManagerPolicy")]
    public IActionResult KOT()
    {
        var categories = _menuService.GetAllCategories();
        var categoryvm = new MenuCategoryVM
        {
            CategoryName = categories.FirstOrDefault().CategoryName,
            menuCategories = categories
        };
        return View(categoryvm);
    }

    public IActionResult GetKOT(int categoryId, string? status)
    {
        ViewBag.Status = status;
        List<OrderVM> category = _accountmanagerorderapp.GetOrderCategoryItem(categoryId, status);
        return PartialView("_KOTCard", category);
    }

    public IActionResult GetOrderDetails([FromBody] List<OrderVM> orderDetailsArray)
    {
        return PartialView("_KOTPrepare", orderDetailsArray);
    }

    public IActionResult MakePrepare([FromBody] List<PrepareItemVM> prepare)
    {
        bool success = _accountmanagerorderapp.UpdateOrderStatus(prepare);

        if (success)
        {
            return Json(new { success = true, message = "Order Status Updated Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    [Authorize(Policy = "AccountManagerPolicy")]
    public IActionResult WaitingList()
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        var sectionvm = sections.Select(Section => new SectionVM
        {
            SectionId = Section.SectionId,
            SectionName = Section.SectionName,
            WaitingList = _accountmanagerorderapp.GetAllWaitingListCustomer(Section.SectionId).Count()
        }).ToList();
        return View(sectionvm);
    }

    public IActionResult GetWaitingListBySectionId(int sectionId)
    {
        var sections = _tableandsectionService.GetAllSection();
        ViewBag.Sections = sections.Select(r => new SelectListItem
        {
            Value = r.SectionId.ToString(),
            Text = r.SectionName
        }).ToList();
        // var tables = _tableandsectionService.Get
        var waitingData = _accountmanagerorderapp.GetAllWaitingListCustomer(sectionId);
        return PartialView("_WaitingListPV", waitingData);
    }

    // public IActionResult GetTableBySectionId(int sectionId)
    // {
    //     var tables = _tableandsectionService.GetAvailableTablesBySectionId(sectionId);
    // }

    public JsonResult GetTableListBySection(int sectionId)
    {
        var tables = _tableandsectionService.GetAvailableTablesBySectionId(sectionId);
        return Json(tables);

    }

    [Authorize(Policy = "AccountManagerPolicy")]
    public IActionResult MenuOrderApp(int customerId)
    {
        var categories = _menuService.GetAllCategories();
        var tables = _accountmanagerorderapp.GetTablesByActiveCustomerId(customerId);
        var orderId = _accountmanagerorderapp.GetOrderIdByCustomerIdActiveOrder(customerId);

        var order = orderId == 0 ? null : _orderService.GetOrderById(orderId);
        var IsCustomerAssociatedWithOrder = _accountmanagerorderapp.IsCustomerAssociatedWithOrder(customerId, orderId);
        if (IsCustomerAssociatedWithOrder)
        {
            ViewBag.CustomerId = customerId;

        }
        if (order != null && order.OrderStatusId != 4 && order.OrderStatusId != 3)
        {
            ViewBag.OrderId = order.OrderId;
            ViewBag.SubTotal = order.SubTotal;
            ViewBag.TotalAmount = order.TotalAmount;
            ViewBag.OrderInstruction = order.OrderInstructions;
        }

        var tax = _accountmanagerorderapp.GetOrderTaxesByOrderId(orderId);
        var alltax = _taxService.GetIsEnabledAndIsDefaultTaxes();
        var IsDefault = _taxService.GetIsEnabledAndNotIsDefaultTaxes();
        var orderDefault = _accountmanagerorderapp.GetOrderTaxesDefaultByOrderId(orderId);
        var categoryvm = new MenuCategoryVM
        {
            CategoryName = categories.FirstOrDefault()?.CategoryName,
            customerTables = tables,
            menuCategories = categories,
            OrderTaxes = (tax.Count == 0) ? alltax.Select(t => new TaxVM
            {
                TaxId = t.TaxId,
                TaxName = t.TaxName,
                TaxAmount = t.TaxAmount,
                TaxTypeId = t.TaxTypeId,
            }).ToList() : tax,
            IsDefaultTaxes = (orderDefault.Count == 0) ? IsDefault.Select(t => new TaxVM
            {
                TaxId = t.TaxId,
                TaxName = t.TaxName,
                TaxAmount = t.TaxAmount,
                TaxTypeId = t.TaxTypeId,
            }).ToList() : orderDefault,
            SgstAmt = order?.SgstAmt,
            OrderComment = order?.OrderInstructions,
            IsSgstInclude = order?.IsSgstInclude,
        };

        return View(categoryvm);
    }

    public IActionResult GetItemByCategory(int CategoryId, string search)
    {
        var items = _menuService.GetItemByCategoryId(CategoryId, search);
        return PartialView("_MenuItemCard", items);
    }

    public IActionResult AddToFavouriteItem(int ItemId)
    {
        bool success = _accountmanagerorderapp.AddToFavouriteItem(ItemId);

        if (success)
        {
            return Json(new { success = true, message = "Item Added to Favourites!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }
    public IActionResult GetFavouriteItem()
    {
        var favouriteItems = _accountmanagerorderapp.GetFavouriteItem();
        return PartialView("_MenuItemCard", favouriteItems);
    }

    public IActionResult DeleteWaitingToken(int waitingId)
    {
        bool success = _accountmanagerorderapp.DeleteWaitingToken(waitingId);

        if (success)
        {
            return Json(new { success = true, message = "Waiting Token Deleted Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult AssignTable(int waitingId, int tableId)
    {
        bool success = _accountmanagerorderapp.AssignTable(waitingId, tableId);
        // var customer = _accountmanagerorderapp.  
        if (success)
        {
            return Json(new { success = true, message = "Table Assigned Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult GetModifierFromItem(int ItemId)
    {
        var combinedModifier = _accountmanagerorderapp.GetItemModifier(ItemId);
        return PartialView("_MenuModifierPV", combinedModifier);
    }

    public IActionResult GetItemInOrder(int CustomerId)
    {
        List<MenuCategoryVM> finalItemData = _accountmanagerorderapp.SetOrderItemDataWithId(CustomerId);
        return PartialView("_OrderDataPV", finalItemData);
    }

    public IActionResult GetMenuItemDetails([FromBody] List<MenuCategoryVM> FinalArrayToBeSubmit)
    {
        var model = _accountmanagerorderapp.SetOrderItemData(FinalArrayToBeSubmit);
        return PartialView("_OrderDataPV", model);
    }

    public IActionResult GetCustomerDetails(int CustomerId)
    {
        var personaldata = _accountmanagerorderapp.GetCustomerDetails(CustomerId);
        return PartialView("_PersonalData", personaldata);
    }

    public IActionResult EditCustomer(CustomerVM customerdata)
    {
        bool success = _accountmanagerorderapp.EditCustomer(customerdata);
        if (success)
        {
            return Json(new { success = true, message = "Table Assigned Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult SaveOrderDetails([FromBody] List<MenuCategoryVM> orderDetailsArray)
    {
        bool success = _accountmanagerorderapp.SaveOrUpdateOrder(orderDetailsArray);

        if (success)
        {
            return Json(new { success = true, message = "Order Saved Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public IActionResult CompleteOrder(int orderId, int customerId, string paymentOption)
    {
        bool success = _accountmanagerorderapp.CompleteOrder(orderId, customerId, paymentOption);

        if (success)
        {
            return Json(new { success = true, message = "Order Completed Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "Order Item is not Ready Yet." });
        }
    }

    public IActionResult CancelOrder(int OrderId, int customerId)
    {
        bool success = _accountmanagerorderapp.CancelOrder(OrderId, customerId);

        if (success)
        {
            return Json(new { success = true, message = "Order Cancelled Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "Item is already ready." });
        }
    }

    public IActionResult SubmitReview(Review review)
    {
        bool success = _accountmanagerorderapp.SubmitReview(review);
        if (success)
        {
            return Json(new { success = true, message = "Review Submitted Successfully!" });
        }
        else
        {
            return Json(new { success = false, message = "An error occurred while processing the request." });
        }
    }










    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}