using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  BLL.Interfaces;
public interface IOrderService
{
    List<Order> GetAllOrders();
    Order GetOrderById(int orderId);
    OrderVM GetOrderByOrderId(int orderId);
    string GetCustomerById(int customerId);
    int GetReviewById(int reviewId);
    string GetPaymentModeById(int paymentId);
    string GetOrderStatusById(int statusId);
    List<OrderStatus> GetAllOrderStatuses();

    IPagedList<OrderVM> GetFilteredOrders(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate);


}