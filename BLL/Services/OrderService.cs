using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using DAL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Entities.ViewModel;
using X.PagedList.Extensions;
using X.PagedList;
using BLL.Interfaces;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    private readonly IMenuRepository _menuRepository;
    private readonly ITableAndSectionRepository _tableAndSectionRepository;

    private readonly ICustomerRepository _customerRepository;
    private readonly ITaxRepository _taxRepository;

    // private readonly IUserService _userService;

    public OrderService(IOrderRepository orderRepository, IMenuRepository menuRepository, ITableAndSectionRepository tableAndSectionRepository, ITaxRepository taxRepository, ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _menuRepository = menuRepository;
        _tableAndSectionRepository = tableAndSectionRepository;
        _taxRepository = taxRepository;
    }


    public List<Order> GetAllOrders()
    {
        return _orderRepository.GetAllOrders();
    }
    public Order GetOrderById(int orderId)
    {
        return _orderRepository.GetOrderById(orderId);
    }
    public string GetCustomerById(int customerId)
    {
        return _orderRepository.GetCustomerById(customerId);
    }
    public int GetReviewById(int reviewId)
    {
        return _orderRepository.GetReviewById(reviewId);
    }
    public string GetPaymentModeById(int paymentId)
    {
        return _orderRepository.GetPaymentModeById(paymentId);
    }
    public string GetOrderStatusById(int statusId)
    {
        return _orderRepository.GetOrderStatusById(statusId);
    }

    public List<OrderStatus> GetAllOrderStatuses()
    {
        return _orderRepository.GetAllOrderStatuses();
    }

    public OrderVM GetOrderByOrderId(int orderId)
    {
        var selectedorder = _orderRepository.GetOrderById(orderId);
        // var tableId = _orderRepository.GetTableIdByCustomerId(selectedorder.CustomerId.Value);
        List<OrderItem> orderitem = _orderRepository.GetOrderItemsByOrderId(orderId);
        List<OrderTax> ordertax = _orderRepository.GetOrderTaxesByOrderId(orderId);
        List<OrderTable> ordertable = _orderRepository.GetOrderTablesByOrderId(orderId);
        var payment = _orderRepository.GetPaymentByOrderId(orderId);
        Console.WriteLine("Hello:" + selectedorder.InvoiceNo);
        Console.WriteLine(selectedorder.OrderItems.Count());
        var orderVM = new OrderVM
        {
            OrderId = selectedorder.OrderId,
            CustomerId = selectedorder.CustomerId,
            Date = selectedorder.Date,
            ReviewId = selectedorder.ReviewId,
            PaymentModeId = selectedorder.PaymentModeId,
            OrderStatusId = selectedorder.OrderStatusId,
            SubTotal = selectedorder.SubTotal,
            TotalAmount = selectedorder.TotalAmount,
            CustomerName = selectedorder.CustomerId.HasValue ? _orderRepository.GetCustomerById(selectedorder.CustomerId.Value) : "No Customer",
            ReviewRating = selectedorder.ReviewId.HasValue ? _orderRepository.GetReviewById(selectedorder.ReviewId.Value) : 0,
            PaymentMode = selectedorder.PaymentModeId.HasValue ? _orderRepository.GetPaymentModeById(selectedorder.PaymentModeId.Value) : "No Payment Mode",
            OrderStatus = selectedorder.OrderStatusId.HasValue ? _orderRepository.GetOrderStatusById(selectedorder.OrderStatusId.Value) : "No Order Status",
            CustomerPhone = selectedorder.CustomerId.HasValue ? _orderRepository.GetCustomerPhoneById(selectedorder.CustomerId.Value) : "No Phone",
            CustomerEmail = selectedorder.CustomerId.HasValue ? _orderRepository.GetCustomerEmailById(selectedorder.CustomerId.Value) : "No Email",
            NoOfPerson = selectedorder.NoOfPerson,
            OrderTables = ordertable.Select(ot => new OrderTableVM
            {
                TableId = (int)ot.TableId,
                TableName = ot.TableId.HasValue ? _tableAndSectionRepository.GetTableNameByTableId(ot.TableId.Value) : "No Table",
                SectionName = ot.TableId.HasValue ? _tableAndSectionRepository.GetSectionNameByTableId(ot.TableId.Value) : "No Section"
            }).ToList(),
            OrderItems = orderitem.Select(oi => new OrderItemVM
            {
                ItemId = (int)oi.ItemId,
                ItemName = oi.ItemId.HasValue ? _menuRepository.GetItemNameById(oi.ItemId.Value) : "No Item",
                Price = oi.Rate,
                Modifiers = _orderRepository.GetOrderModifiersByOrderItemIdAndItemId(oi.OrderItemId, (int)oi.ItemId).Select(om => new OrderModifierVM
                {
                    ModifierId = (int)om.ModifierId,
                    ModifierName = om.ModifierId.HasValue ? _menuRepository.GetModifierNameById(om.ModifierId.Value) : "No Modifier",
                    ModifierRate = om.Rate,
                    Quantity = 1
                }).ToList(),
                Quantity = oi.Quantity
            }).ToList(),
            OrderTax = ordertax.Select(ot => new OrderTaxVM
            {
                TaxId = ot.TaxId ?? 0,
                TaxTypeId = ot.TaxId.HasValue ? _taxRepository.GetTexTypeIdByTaxId(ot.TaxId.Value) : 0,
                TaxName = ot.TaxId.HasValue ? _taxRepository.GetTaxNameById(ot.TaxId.Value) : "No Tax",
                TaxAmount = (decimal)(ot.TaxRate ?? 0)
            }).ToList(),
            InvoiceNo = selectedorder.InvoiceNo ?? "NULL",
            Duration = selectedorder.OrderDuration ?? TimeSpan.Zero,
        };
        return orderVM;
    }



    public IPagedList<OrderVM> GetFilteredOrders(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        var orders = _orderRepository.GetAllOrders().AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            orders = orders.Where(u => u.OrderId.ToString().ToLower().Contains(searchLower));
        }

        Console.WriteLine("Order Status: " + orderStatus);
        var OrderStatusId = _orderRepository.GetOrderStatusIdByStatusName(orderStatus);
        // var orderStatus12 = orders.Where(u => u.OrderStatus.OrderStatusName.ToString().ToLower().Contains(orderStatus.ToLower()));
        // Console.WriteLine(orderStatus12.Count());


        // FIltering By status
        if (!string.IsNullOrEmpty(OrderStatusId.ToString()) && OrderStatusId != 0)
        {
            orders = orders.Where(u => u.OrderStatusId.ToString().ToLower().Contains(OrderStatusId.ToString().ToLower()));
        }

        // Filtering by date range
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
        {
            orders = orders.Where(o => o.Date >= DateOnly.FromDateTime(start));
        }
        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
        {
            orders = orders.Where(o => o.Date <= DateOnly.FromDateTime(end));
        }

        //Filter By date
        if (!string.IsNullOrEmpty(filterdate))
        {
            DateTime now = DateTime.UtcNow;

            switch (filterdate)
            {
                case "Recent":
                    orders = orders.OrderByDescending(o => o.Date).Take(5);
                    break;
                case "Last 7 days":
                    orders = orders.Where(o => o.Date >= DateOnly.FromDateTime(now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    orders = orders.Where(o => o.Date >= DateOnly.FromDateTime(now.AddDays(-30)));
                    break;
                case "Current Month":
                    orders = orders.Where(o => o.Date.Month == now.Month && o.Date.Year == now.Year);
                    break;
                case "Current Year":
                    orders = orders.Where(o => o.Date.Year == now.Year);
                    break;
                case "All time":
                default:
                    // No filter needed, return all orders
                    break;
            }
        }

        //Sorting

        // Sorting
        switch (filterOptions.SortBy)
        {
            case "Order":
                if (filterOptions.IsAsc == true)
                {
                    orders = orders.OrderBy(c => c.OrderId);
                }
                else
                {
                    orders = orders.OrderByDescending(c => c.OrderId);
                }
                break;

            case "Date":
                orders = (filterOptions.IsAsc ?? false) ? orders.OrderBy(c => c.Date) : orders.OrderByDescending(c => c.Date);
                break;

            case "Customer":
                orders = (filterOptions.IsAsc ?? false)
                    ? orders.OrderBy(c => _customerRepository.GetCustomerById(c.CustomerId ?? 0).Name)
                    : orders.OrderByDescending(c => _customerRepository.GetCustomerById(c.CustomerId ?? 0).Name);
                break;

            case "TotalAmount":
                orders = (filterOptions.IsAsc ?? false) ? orders.OrderBy(c => c.TotalAmount) : orders.OrderByDescending(c => c.TotalAmount);
                break;

            default:
                orders = orders.OrderByDescending(c => c.Date);
                break;
        }

        // Get total count and handle page size dynamically
        int totalTables = orders.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var paginatedtables = orders
           .Select(order => new OrderVM
           {
               OrderId = order.OrderId,
               CustomerId = order.CustomerId,
               Date = order.Date,
               ReviewId = order.ReviewId,
               PaymentModeId = order.PaymentModeId,
               OrderStatusId = order.OrderStatusId,
               TotalAmount = order.TotalAmount,
               CustomerName = order.CustomerId.HasValue ? _orderRepository.GetCustomerById(order.CustomerId.Value) : "No Customer",
               ReviewRating = order.ReviewId.HasValue ? _orderRepository.GetReviewById(order.ReviewId.Value) : 0,
               PaymentMode = order.PaymentModeId.HasValue ? _orderRepository.GetPaymentModeById(order.PaymentModeId.Value) : "No Payment Mode",
               OrderStatus = order.OrderStatusId.HasValue ? _orderRepository.GetOrderStatusById(order.OrderStatusId.Value) : "No Order Status"
           }).ToPagedList(filterOptions.Page ?? 1, filterOptions.PageSize);


        return paginatedtables;

    }
}