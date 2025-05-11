using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  BLL.Interfaces;
public interface ICustomerService
{

    List<Customer> GetAllCustomers();
    CustomerVM GetCustomerByCustomerId(int customerId);

    

    IPagedList<CustomerVM> GetFilteredCustomers(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate);

}