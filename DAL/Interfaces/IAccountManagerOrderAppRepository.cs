using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  DAL.Interfaces;

public interface IAccountManagerOrderAppRepository
{
    List<OrderSectionVM> GetOrderSection();

    List<WaitingListVM> GetAllWaitingListCustomer(int sectionId);

    Order GetOrderItemById(int orderId);

    List<CustomerTable> GetTablesByCustomerId(int customerId);

    List<CustomerTable> GetTablesByActiveCustomerId(int customerId);

    List<OrderTax> GetOrderTaxesByOrderId(int OrderId);

    List<OrderTax> GetOrderTaxesDefaultByOrderId(int OrderId);
    Customer GetCustomer(int customerId);

    OrderItem GetOrderItem(int orderId, int itemId);

    public int GetCustomerIDByName(string email);

    bool EditCustomer(Customer customerdata);

    void UpdateWaitingList(WaitingListVM waitingList);

    WaitingListVM GetWaitingData(int waitingId);

    WaitingListVM GetWaitingDataByEmail(string email);
    Customer IsCustomer(string email);

    WaitingList IsInWaitingList(string email);

    WaitingList GetWaitingListBySectionId(int sectionId);

    void AddInWaitingList(WaitingList customerTable);

    void DeleteCustomerFromWaitingList(string email);

    List<OrderVM> GetOrderCategoryItem(int categoryId, string? status);

    bool PrepareItem(List<PrepareItemVM> prepareItems);

    bool AddToFavouriteItem(Favourite favouriteItem);
    List<MenuCategoryVM> GetFavouriteItems();

    void DeleteOrderItem(int OrderItemId);

    void DeleteOrderModifier(int OrderModifierId);

    bool DeleteWaitingToken(int waitingId);

    void DeleteOrderTax(int OrderTaxId);

    bool IsCustomerAssociatedWithOrder(int customerId, int orderId);

    void UpdateCustomerTableStatus(int tableId);

    void AddReview(Review review);

    void AddOrderTable(OrderTable orderTable);

}