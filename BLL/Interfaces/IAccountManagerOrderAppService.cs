using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace  BLL.Interfaces;

public interface IAccountManagerOrderAppService
{
    List<OrderSectionVM> GetOrderSection();

    List<WaitingListVM> GetAllWaitingListCustomer(int sectionId);

    public int GetCustomerIDByName(string email);

    List<MenuCategoryVM> SetOrderItemData(List<MenuCategoryVM> ItemDetail);

    List<MenuCategoryVM> SetOrderItemDataWithId(int CustomerId);

    List<TaxVM> GetOrderTaxesByOrderId(int OrderId);

     List<TaxVM> GetOrderTaxesDefaultByOrderId(int OrderId);

    int GetOrderIdByCustomerId(int customerId);

    int GetOrderIdByCustomerIdActiveOrder(int customerId);

    WaitingListVM GetWaitingTokenById(int waitingId);

    List<CustomerTableVM> GetTablesByCustomerId(int customerId);

    List<CustomerTableVM> GetTablesByActiveCustomerId(int customerId);

    CustomerVM GetCustomerDetails(int customerId);

    WaitingListVM GetWaitingData(int waitingId);

    WaitingListVM GetWaitingDataByEmail(string email);

    Customer IsCustomer(string email);

    bool AddOrUpdateCustomer([FromBody] JsonObject customerdata);

    bool EditCustomer(CustomerVM customerdata);

    bool AddInWaitingList([FromBody] JsonObject customerdata);

    List<OrderVM> GetOrderCategoryItem(int categoryId, string? status);

    bool UpdateOrderStatus(List<PrepareItemVM> prepare);

    bool AddToFavouriteItem(int ItemId);

    List<MenuCategoryVM> GetFavouriteItem();

    bool DeleteWaitingToken(int waitingId);

    bool AssignTable(int waitingId, int tableId);

    public MenuCategoryVM GetItemModifier(int ItemId);

    bool SaveOrUpdateOrder(List<MenuCategoryVM> orderDetailsArray);

    bool CompleteOrder(int ordreid, int customerId, string paymentOption);

    bool IsCustomerAssociatedWithOrder(int customerId, int orderId);

    bool CancelOrder(int ordreid, int customerId);

    bool SubmitReview(Review review);
}