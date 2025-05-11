using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace DAL.Interfaces;

public interface IOrderRepository
{
    Order GetOrderById(int orderId);
    List<Order> GetAllOrders();
    string GetCustomerById(int customerId);

    string GetCustomerEmailById(int customerId);

    int GetPaymentModeIdByMode(string mode);

    string GetCustomerPhoneById(int customerId);

    int GetTableIdByCustomerId(int customerId);

    int GetReviewById(int reviewId);
    string GetPaymentModeById(int paymentId);
    string GetOrderStatusById(int statusId);

    Payment GetPaymentByOrderId(int orderId);

    List<OrderItem> GetOrderItemsByOrderId(int orderId);

    List<OrderTable> GetOrderTablesByOrderId(int orderId);

    List<OrderTax> GetOrderTaxesByOrderId(int orderId);

    List<OrderModifier> GetOrderModifiersByOrderItemIdAndItemId(int orderItemId, int itemId);
    List<OrderStatus> GetAllOrderStatuses();
    int GetOrderStatusIdByStatusName(string statusName);

    void AddOrder(Order order);

    void UpdateOrder(Order order);

    void AddOrderItem(OrderItem orderItem);

    void UpdateOrderItem(OrderItem orderItem);

    void UpdateOrderModifier(OrderModifier orderModifier);

    void AddOrderModifier(OrderModifier orderModifier);

    void AddOrderTax(OrderTax orderTax);
    void UpdateOrderTax(OrderTax orderTax);

    void AddPayment(Payment payment);

}