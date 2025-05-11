using  DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace  DAL.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly PizzaShopContext _context;

    public OrderRepository(PizzaShopContext context)
    {
        _context = context;
    }
    

    public List<Order> GetAllOrders()
    {
        return _context.Orders.OrderByDescending(c => c.OrderId).ToList();
    }
    public Order GetOrderById(int orderId)
    {
        return _context.Orders.FirstOrDefault(r => r.OrderId == orderId);
    }

    public string GetCustomerById(int customerId)
    {
        var customername = (from customer in _context.Customers
                           where customer.CustomerId == customerId
                           select customer.Name).FirstOrDefault();
        return customername;
    }
    
    public int GetPaymentModeIdByMode(string mode)
    {
        var paymentmodeid = (from payment in _context.PaymentModes
                           where payment.PaymentModeName == mode
                           select payment.PaymentModeId).FirstOrDefault();
        return paymentmodeid;
    }

    public string GetCustomerEmailById(int customerId)
    {
        var customeremail = (from customer in _context.Customers
                             where customer.CustomerId == customerId
                             select customer.Email).FirstOrDefault();
        return customeremail;
    }

    public string GetCustomerPhoneById(int customerId)
    {
        var customerphone = (from customer in _context.Customers
                           where customer.CustomerId == customerId
                           select customer.Phone).FirstOrDefault();
        return customerphone;
    }

    public int GetTableIdByCustomerId(int customerId)
    {
        var tableid = (from customer in _context.Customers
                           where customer.CustomerId == customerId
                           select customer.TableId).FirstOrDefault();
        return (int)tableid;
    }

    public int GetReviewById(int reviewId)
    {
        var reviewrating = (from review in _context.Reviews
                           where review.ReviewId == reviewId
                           select review.Rating).FirstOrDefault();
        return (int)reviewrating;
    }

    public string GetPaymentModeById(int paymentId)
    {
        var paymentmode = (from payment in _context.PaymentModes
                           where payment.PaymentModeId == paymentId
                           select payment.PaymentModeName).FirstOrDefault();
        return paymentmode;
    }

    public string GetOrderStatusById(int statusId)
    {
        var orderstatus = (from status in _context.OrderStatuses
                           where status.OrderStatusId == statusId
                           select status.OrderStatusName).FirstOrDefault();
        return orderstatus;
    }

    public List<OrderItem> GetOrderItemsByOrderId(int orderId)
    {
        return _context.OrderItems.Where(r => r.OrderId == orderId).ToList();
    }

    public List<OrderTable> GetOrderTablesByOrderId(int orderId)
    {
        return _context.OrderTables.Where(r => r.OrderId == orderId).ToList();
    }


    public List<OrderModifier> GetOrderModifiersByOrderItemIdAndItemId(int orderItemId, int itemId)
    {
        return _context.OrderModifiers.Where(r => r.OrderItemId == orderItemId && r.ItemId == itemId).ToList();
    }
 
    public List<OrderTax> GetOrderTaxesByOrderId(int orderId)
    {
        return _context.OrderTaxes.Where(r => r.OrderId == orderId).ToList();
    }
    public List<OrderStatus> GetAllOrderStatuses()
    {
        return _context.OrderStatuses.ToList();
    }

    public Payment GetPaymentByOrderId(int orderId)
    {
        return _context.Payments.FirstOrDefault(u => u.OrderId == orderId);
    }
    
    public int GetOrderStatusIdByStatusName(string statusName)
    {
        var orderstatusid = (from status in _context.OrderStatuses
                           where status.OrderStatusName == statusName
                           select status.OrderStatusId).FirstOrDefault();
        return orderstatusid;
    }

    public void AddOrder(Order order)
    {
        _context.Orders.Add(order);
        Save();
    }
    public void UpdateOrder(Order order)
    {
        _context.Orders.Update(order);
        Save();
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        Save();
    }
    public void UpdateOrderItem(OrderItem orderItem)
    {
        _context.OrderItems.Update(orderItem);
        Save();
    }

    public void AddOrderModifier(OrderModifier orderModifier)
    {
        _context.OrderModifiers.Add(orderModifier);
        Save();
    }
    public void UpdateOrderModifier(OrderModifier orderModifier)
    {
        _context.OrderModifiers.Update(orderModifier);
        Save();
    }

    public void AddOrderTax(OrderTax orderTax)
    {
        _context.OrderTaxes.Add(orderTax);
        Save();
    }
    public void UpdateOrderTax(OrderTax orderTax)
    {
        _context.OrderTaxes.Update(orderTax);
        Save();
    }

    public void AddPayment(Payment payment)
    {
        _context.Payments.Add(payment);
        Save();
    }
    public void Save()
    {
        _context.SaveChanges();
    }

  
    // public List<TaxType> GetAllTaxTypes()
    // {
    //     return _context.TaxTypes.ToList();
    // }

    // public string GetTaxTypeNameById(int statusId)
    // {
    //     var taxtypename = (from tax in _context.TaxTypes
    //                        where tax.TaxTypeId == statusId
    //                        select tax.TaxTypeName).FirstOrDefault();
    //     return taxtypename;
    // }

    // public Taxis GetTaxById(int taxId)
    // {
    //     return _context.Taxes.FirstOrDefault(r => r.TaxId == taxId);
    // }




}