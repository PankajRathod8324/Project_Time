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
using System.Linq;
using BLL.Interfaces;

namespace BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    private readonly IOrderRepository _orderRepository;



    public CustomerService(ICustomerRepository customerRepository, IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }


    public List<Customer> GetAllCustomers()
    {
        return _customerRepository.GetAllCustomers();
    }
    // public Customer GetCustomersById(int customerId)
    // {
    //     return _customerRepository.GetCustomerById(customerId);
    // }

    public CustomerVM GetCustomerByCustomerId(int customerId)
    {
        var selectedcustomer = _customerRepository.GetCustomerById(customerId);
        var selectedorder = _customerRepository.GetOrderIdByCustomerId(customerId);
        var orders = _customerRepository.GetOrderByCustomerId(customerId);
        // List<OrderItem> orderitem = _orderRepository.GetOrderItemsByOrderId(selectedorder).OrderByDescending(oi => oi.OrderItemId).ToList();

        // List<OrderTax> ordertax = _orderRepository.GetOrderTaxesByOrderId(orderId);
        // List<OrderTable> ordertable = _orderRepository.GetOrderTablesByOrderId(orderId);
        // var payment = _orderRepository.GetPaymentByOrderId(orderId);
        // Console.WriteLine("Hello:"+selectedorder.InvoiceNo);
        // Console.WriteLine(selectedorder.OrderItems.Count());
        var customerVM = new CustomerVM
        {
            CustomerId = selectedcustomer.CustomerId,
            Name = selectedcustomer.Name,
            Phone = selectedcustomer.Phone,
            MaxOrder = _customerRepository.GetMaxOrderValue(selectedcustomer.CustomerId),
            AverageOrder = Math.Round(_customerRepository.GetAverageOrderValue(selectedcustomer.CustomerId), 2),
            Visits = _customerRepository.GetOrderByCustomerId(selectedcustomer.CustomerId).Count(),
            LastVisit = _customerRepository.GetLastVisit(selectedcustomer.CustomerId),
            Orders = orders.Select(order => new OrderVM
            {
                OrderDate = DateOnly.FromDateTime(order.CreatedAt),
                OrderType = order.OrderType == null ? "DineIn" : "TakeAway",
                PaymentStatus = _customerRepository.GetPaymentStatusNameByOrderId(order.OrderId) == null ? "Pending" : _customerRepository.GetPaymentStatusNameByOrderId(order.OrderId),
                ItemCount = _orderRepository.GetOrderItemsByOrderId(order.OrderId).Count(),
                OrderAmount = order.TotalAmount
            }).ToList()
        };
        return customerVM;
    }



    public IPagedList<CustomerVM> GetFilteredCustomers(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        var customers = _customerRepository.GetAllCustomers().AsQueryable();

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search.ToLower();
            customers = customers.Where(u => u.Name.ToString().ToLower().Contains(searchLower));
        }


        // var orderStatus12 = orders.Where(u => u.OrderStatus.OrderStatusName.ToString().ToLower().Contains(orderStatus.ToLower()));
        // Console.WriteLine(orderStatus12.Count());




        // Filtering by date range
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
        {
            customers = customers.Where(o => o.Date >= DateOnly.FromDateTime(start));
        }
        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
        {
            customers = customers.Where(o => o.Date <= DateOnly.FromDateTime(end));
        }

        // Filter By date
        if (!string.IsNullOrEmpty(filterdate))
        {
            DateTime now = DateTime.UtcNow;

            switch (filterdate)
            {
                case "Recent":
                    customers = customers.OrderByDescending(o => o.Date).Take(5);
                    break;
                case "Last 7 days":
                    customers = customers.Where(o => o.Date >= DateOnly.FromDateTime(now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    customers = customers.Where(o => o.Date >= DateOnly.FromDateTime(now.AddDays(-30)));
                    break;
                case "Current Month":
                    customers = customers.Where(o => o.Date.HasValue && o.Date.Value.Month == now.Month && o.Date.Value.Year == now.Year);
                    break;
                case "Current Year":
                    customers = customers.Where(o => o.Date.HasValue && o.Date.Value.Year == now.Year);
                    break;
                case "All time":
                default:
                    // No filter needed, return all orders
                    break;
            }
        }

        //Sorting

        switch (filterOptions.SortBy)
        {
            case "Name":
                if (filterOptions.IsAsc == true)
                {
                    customers = customers.OrderBy(c => c.Name);
                }
                else
                {
                    customers = customers.OrderByDescending(c => c.Name);
                }
                break;

            case "Date":
                customers = filterOptions.IsAsc.GetValueOrDefault() ? customers.OrderBy(c => c.Date) : customers.OrderByDescending(c => c.Date);
                break;

            case "TotalOrder":
                customers = filterOptions.IsAsc.GetValueOrDefault()
                    ? customers.OrderBy(c => _customerRepository.GetOrderByCustomerId(c.CustomerId).Count())
                    : customers.OrderByDescending(c => _customerRepository.GetOrderByCustomerId(c.CustomerId).Count());
                break;

            default:
                customers = customers.OrderByDescending(c => c.Date);
                break;
        }


        // Get total count and handle page size dynamically
        int totalTables = customers.Count();
        int pageSize = filterOptions.PageSize > 0 ? Math.Min(filterOptions.PageSize, totalTables) : 10; // Default 10

        var paginatedcustomers = customers
           .Select(customer => new CustomerVM
           {
               CustomerId = customer.CustomerId,
               Name = customer.Name,
               Email = customer.Email,
               Phone = customer.Email,
               TotalOrder = _customerRepository.GetOrderByCustomerId(customer.CustomerId).Count(),
               Date = customer.Date,
           }).ToPagedList(filterOptions.Page ?? 1, filterOptions.PageSize > 0 ? filterOptions.PageSize : 10);


        return paginatedcustomers;

    }

}