using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace DAL.Interfaces;

public interface ICustomerRepository
{
   List<Customer> GetAllCustomers();

   List<Order> GetOrderByCustomerId(int customerId);

   int GetOrderIdByCustomerId(int customerId);

   int GetNoOfPersons(int customerId);
   Customer GetCustomerById(int customerId);

   public int GetOrderIdByCustomerIdActiveOrder(int customerId);

   List<CustomerTable> GetOrderTableByCustomerId(int customerId);

   decimal GetMaxOrderValue(int customerId);

   decimal GetAverageOrderValue(int customerId);

   DateTime GetLastVisit(int customerId);

   string GetPaymentStatusNameByOrderId(int orderId);

   void AddCustomer(Customer customer);

   void UpdateCustomer(Customer customer);

   void AddCustomerTable(CustomerTable customerTable);
}