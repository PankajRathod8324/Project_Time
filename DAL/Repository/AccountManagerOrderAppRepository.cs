using DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AccountManagerOrderAppRepository : IAccountManagerOrderAppRepository
{
    private readonly PizzaShopContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IMenuRepository _menuRepository;
    public AccountManagerOrderAppRepository(PizzaShopContext context, IMenuRepository menuRepository, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _menuRepository = menuRepository;
        _httpContextAccessor = httpContextAccessor;
    }




    public List<OrderSectionVM> GetOrderSection()
    {
        var activeTableOrders = _context.Tables
            .Include(t => t.Status)
            .Include(t => t.CustomerTables)
            .ThenInclude(ct => ct.Customer)
            .ToList();

        var tableInfoList = activeTableOrders.Select(table =>
        {
            var activeCT = table.CustomerTables.FirstOrDefault(ct => (bool)ct.IsActive);
            var activeOrder = activeCT != null
                ? _context.Orders.FirstOrDefault(order =>
                    order.CustomerId == activeCT.CustomerId &&
                    order.OrderStatusId == 6)
                : null;

            return new
            {
                Table = table,
                Order = activeOrder
            };
        }).ToList();


        var groupedByOrder = tableInfoList
      .GroupBy(x => x.Order?.CustomerId)
      .SelectMany(group =>
      {
          if (group.Key != null && group.Count() > 1)
          {
              // Multiple tables with same OrderId merge them
              var first = group.First();
              return new List<OrderTableVM>
              {
                new OrderTableVM
                {
                    SectionId = first.Table.SectionId ?? 0,
                    TableId = 0,
                    TableName = string.Join(", ", group.Select(x => x.Table.TableName).Distinct()),
                    Status = string.Join(", ", group.Select(x => x.Table.Status.StatusName).Distinct()),
                    Capacity = group.Sum(x => x.Table.Capacity),
                    OccuipiedTime = first.Order != null ? (DateTime.UtcNow - first.Order.CreatedAt) : TimeSpan.Zero,
                    OrderTable = new List<OrderTableVM>
                    {
                        new OrderTableVM
                        {
                            OrderId = first.Order.OrderId,
                            Order = new List<OrderVM>
                            {
                                new OrderVM
                                {
                                    OrderId = first.Order.OrderId,
                                    OrderAmount = first.Order.TotalAmount,
                                    CustomerId = first.Order.CustomerId,
                                    OccupiedTime = DateTime.UtcNow - first.Order.CreatedAt
                                }
                            }
                        }
                    },
                    CustomerTables = group
                        .SelectMany(x => x.Table.CustomerTables.Where(ct => (bool)ct.IsActive))
                        .Select(ct => new CustomerTable
                        {
                            CustomerId = ct.CustomerId
                        })
                        .Distinct()
                        .ToList()
                }
              };
          }
          else
          {
              // Each table shown separately (either no order or unique order)
              return group.Select(x => new OrderTableVM
              {
                  SectionId = x.Table.SectionId ?? 0,
                  TableId = x.Table.TableId,
                  TableName = x.Table.TableName,
                  Status = x.Table.Status.StatusName,
                  Capacity = x.Table.Capacity,
                  OccuipiedTime = x.Order != null ? (DateTime.UtcNow - x.Order.CreatedAt) : TimeSpan.Zero,
                  OrderTable = x.Order != null ? new List<OrderTableVM>
                  {
                    new OrderTableVM
                    {
                        OrderId = x.Order.OrderId,
                        Order = new List<OrderVM>
                        {
                            new OrderVM
                            {
                                OrderId = x.Order.OrderId,
                                OrderAmount = x.Order.TotalAmount,
                                CustomerId = x.Order.CustomerId,
                                OccupiedTime = DateTime.UtcNow - x.Order.CreatedAt
                            }
                        }
                    }
                  } : new List<OrderTableVM>(),
                  CustomerTables = x.Table.CustomerTables
                      .Where(ct => (bool)ct.IsActive)
                      .Select(ct => new CustomerTable
                      {
                          CustomerId = ct.CustomerId
                      })
                      .ToList()
              });
          }
      })
      .ToList();


        var sectionData = _context.Sections
            .AsEnumerable()
            .Select(section => new OrderSectionVM
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                Available = section.Tables.Count(t => t.Status.StatusName == "Available"),
                Assigned = section.Tables.Count(t => t.Status.StatusName == "Assigned"),
                Running = section.Tables.Count(t => t.Status.StatusName == "Running"),
                Reserved = section.Tables.Count(t => t.Status.StatusName == "Reserved"),
                Table = groupedByOrder
                    .Where(t => t.SectionId == section.SectionId)
                    .ToList()
            }).ToList();


        return sectionData;
    }

    public List<CustomerTable> GetTablesByCustomerId(int customerId)
    {
        return _context.CustomerTables.Where(c => c.CustomerId == customerId).ToList();
    }

    public List<CustomerTable> GetTablesByActiveCustomerId(int customerId)
    {
        return _context.CustomerTables.Where(c => c.CustomerId == customerId && c.IsActive == true).ToList();
    }

    public List<OrderTax> GetOrderTaxesByOrderId(int OrderId)
    {
        var orderTaxes = _context.OrderTaxes
            .Include(ot => ot.Tax)
            .Where(ot => ot.OrderId == OrderId && ot.Tax.IsEnabled == true && ot.Tax.IsDefault == true).ToList();

        return orderTaxes;
    }

    public List<OrderTax> GetOrderTaxesDefaultByOrderId(int OrderId)
    {
        var orderTaxes = _context.OrderTaxes
           .Include(ot => ot.Tax)
           .Where(ot => ot.OrderId == OrderId && ot.Tax.IsEnabled == true && ot.Tax.IsDefault == false).ToList();

        return orderTaxes;
    }

    // check customer that is associated with orderid or not 
    public bool IsCustomerAssociatedWithOrder(int customerId, int orderId)
    {
        if (orderId != 0)
        {
            var order = _context.Orders
           .Include(o => o.Customer)
           .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId && o.OrderStatusId != 4 && o.OrderStatusId != 3);
            if (order != null)
            {
                return true; // Customer is associated with the order
            }
            var customerTable = _context.CustomerTables
          .Include(ct => ct.Table)
          .FirstOrDefault(ct => ct.CustomerId == customerId && ct.IsActive == true);
            if (customerTable != null)
            {
                return true; // Customer is associated with the order
            }
        }
        else
        {
            var customerTable = _context.CustomerTables
          .Include(ct => ct.Table)
          .FirstOrDefault(ct => ct.CustomerId == customerId && ct.IsActive == true);
            if (customerTable != null)
            {
                return true; // Customer is associated with the order
            }
        }




        return false;

    }

    public OrderItem GetOrderItem(int orderId, int itemId)
    {
        var orderItem = _context.OrderItems
            .Include(oi => oi.OrderModifiers)
            .FirstOrDefault(oi => oi.OrderId == orderId && oi.ItemId == itemId);

        return orderItem;
    }
    public int GetCustomerIDByName(string email)
    {
        var userId = (from user in _context.Customers
                      where user.Email == email
                      select user.CustomerId).FirstOrDefault();
        return userId;
    }

    public List<WaitingListVM> GetAllWaitingListCustomer(int sectionId)
    {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var waitingListData = _context.WaitingLists
            .Where(sectionId == 0 ? temp => temp.IsDeleted == false : temp => temp.SectionId == sectionId && temp.IsDeleted == false)
            .Select(temp => new WaitingListVM
            {
                SectionId = temp.SectionId,
                Name = temp.Name,
                WaitingListId = temp.WaitingListId,
                Email = temp.Email,
                NoOfPerson = temp.NoOfPerson,
                Phone = temp.Phone,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc((DateTime)temp.CreatedAt, timeZone),
                Duration = DateTime.Now - TimeZoneInfo.ConvertTimeFromUtc((DateTime)temp.CreatedAt, timeZone),
            }).ToList();
        return waitingListData;
    }

    public WaitingListVM GetWaitingData(int waitingId)
    {
        var waitingData = _context.WaitingLists.Where(waiting => waiting.WaitingListId == waitingId && waiting.IsDeleted == false).FirstOrDefault();
        if (waitingData == null)
        {
            return null;
        }
        var waitingvm = new WaitingListVM
        {
            SectionId = waitingData.SectionId,
            Name = waitingData.Name,
            WaitingListId = waitingData.WaitingListId,
            Email = waitingData.Email,
            NoOfPerson = waitingData.NoOfPerson,
            Phone = waitingData.Phone,
            IsDeleted = (bool)waitingData.IsDeleted
        };
        return waitingvm;
    }

    public WaitingListVM GetWaitingDataByEmail(string email)
    {
        var waitingData = _context.Customers.Where(waiting => waiting.Email == email).FirstOrDefault();
        if (email == null)
        {
            return null;
        }
        var waitingvm = new WaitingListVM
        {
            SectionId = _context.Tables.Where(t => t.TableId == waitingData.TableId).FirstOrDefault().SectionId ?? 0,
            Name = waitingData.Name,
            Email = waitingData.Email,
            NoOfPerson = (int)waitingData.Noofperson,
            Phone = waitingData.Phone,
        };

        return waitingvm;
    }
    public WaitingList GetWaitingListBySectionId(int sectionId)
    {
        var waitingList = _context.WaitingLists
            .Where(w => w.SectionId == sectionId && w.IsDeleted == false)
            .ToList();

        return waitingList.FirstOrDefault();
    }

    public Customer IsCustomer(string email)
    {
        return _context.Customers.FirstOrDefault(c => c.Email == email);
    }

    public Customer GetCustomer(int customerId)
    {
        return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
    }

    public WaitingList IsInWaitingList(string email)
    {
        return _context.WaitingLists.FirstOrDefault(c => c.Email == email && c.IsDeleted == false);
    }

    public void AddInWaitingList(WaitingList customerTable)
    {
        _context.WaitingLists.Add(customerTable);
        Save();
    }

    public void DeleteCustomerFromWaitingList(string email)
    {
        var existingItem = _context.WaitingLists.FirstOrDefault(m => m.Email == email);
        existingItem.IsDeleted = true;
        Save();
    }

    public List<OrderVM> GetOrderCategoryItem(int categoryId, string? status)
    {
        var ordersQuery = _context.Orders.AsQueryable();

        if (categoryId != 0)
        {
            ordersQuery = ordersQuery.Where(o => _context.OrderItems.Any(oi => oi.OrderId == o.OrderId && o.OrderStatusId != 4 && o.OrderStatusId != 3 && oi.CategoryId == categoryId && oi.ItemId.HasValue
                                                                               && _context.MenuItems.Any(mi => mi.ItemId == oi.ItemId
                                                                                                               && mi.CategoryId == categoryId)));
        }
        else
        {
            ordersQuery = ordersQuery.Where(o => _context.OrderItems.Any(oi => oi.OrderId == o.OrderId && o.OrderStatusId != 4 && o.OrderStatusId != 3 && oi.ItemId.HasValue
                                                                               && _context.MenuItems.Any(mi => mi.ItemId == oi.ItemId)));
        }


        var orders = categoryId != 0 ? ordersQuery
            .Include(o => o.OrderItems
                .Where(oi => oi.CategoryId == categoryId))
            .ThenInclude(oi => oi.OrderModifiers)
            .ToList() : ordersQuery
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.OrderModifiers)
            .ToList();


        var orderVm = orders.Select(o => new OrderVM
        {
            OrderId = o.OrderId,
            OccupiedTime = DateTime.UtcNow - o.CreatedAt,
            OrderInstruction = o.OrderInstructions,
            OrderItems = o.OrderItems
                .Where(oi => oi.ItemId.HasValue && ((status == "In Progress" && (oi.Quantity - oi.Readyitemquantity) > 0) ||
                                  (status != "In Progress" && oi.Readyitemquantity > 0)) && _context.MenuItems.Any(mi => mi.ItemId == oi.ItemId))
                .SelectMany(oi => new List<OrderItemVM>
                {
                new OrderItemVM
                {
                    ItemId = (int)oi.ItemId,
                    ItemName = _context.MenuItems.Where(mi => mi.ItemId == oi.ItemId).Select(mi => mi.ItemName).FirstOrDefault(),
                    Price = oi.Rate,
                    Quantity = (status == "In Progress") ? oi.Quantity - oi.Readyitemquantity : oi.Readyitemquantity,
                    ItemInstructions = oi.ItemInstructions,
                    Status = status,
                    Modifiers = oi.OrderModifiers.Select(om => new OrderModifierVM
                    {
                        ModifierId = (int)om.ModifierId,
                        ModifierName = _context.MenuModifiers.Where(mod => mod.ModifierId == om.ModifierId).Select(mod => mod.ModifierName).FirstOrDefault(),
                        ModifierRate = om.Rate,
                        Quantity = 1
                    }).ToList()
                }

                }).ToList(),
            CustomerTables = _context.CustomerTables.Where(r => r.CustomerId == o.CustomerId && r.IsActive == true).Select(ot => new CustomerTableVM
            {
                TableId = (int)ot.TableId,
                TableName = ot.TableId.HasValue ? (from table in _context.Tables
                                                   where table.TableId == ot.TableId
                                                   select table.TableName).FirstOrDefault() : "No Table",
                SectionName = ot.TableId.HasValue ? (from table in _context.Tables
                                                     join section in _context.Sections on table.SectionId equals section.SectionId
                                                     where table.TableId == ot.TableId
                                                     select section.SectionName).FirstOrDefault() : "No Section"
            }).ToList(),

        }).Where(o => o.OrderItems.Count > 0).ToList();

        return orderVm;
    }

    public bool PrepareItem(List<PrepareItemVM> prepareItems)
    {
        foreach (var item in prepareItems)
        {
            var orderItem = _context.OrderItems
                .FirstOrDefault(x => x.OrderId == item.OrderId && x.ItemId == item.ItemId);
            if (item.Status == "In Progress")
            {
                if (orderItem != null)
                {
                    orderItem.Readyitemquantity += item.Quantity;
                }
            }
            else
            {
                if (orderItem != null)
                {
                    orderItem.Readyitemquantity -= item.Quantity;
                }
            }
        }

        _context.SaveChanges();

        foreach (var item in prepareItems)
        {
            var orderItem = _context.OrderItems
                .FirstOrDefault(x => x.OrderId == item.OrderId && x.ItemId == item.ItemId);


            if (orderItem.Quantity == orderItem.Readyitemquantity)
            {
                orderItem.Status = "Ready";
            }
            else
            {
                orderItem.Status = "In Progress";
            }
        }
        _context.SaveChanges();



        return true;
    }

    public List<MenuCategoryVM> GetFavouriteItems()
    {
        string useremail = _httpContextAccessor.HttpContext.Request.Cookies["Email"];
        var user = _context.Users.FirstOrDefault(u => u.Email == useremail);
        var favouriteItems = _context.Favourites
            .Where(f => f.UserId == user.UserId)
            .Select(f => f.ItemId)
            .ToList();

        var menuItems = _context.MenuItems
            .Where(m => favouriteItems.Contains(m.ItemId))
            .ToList();

        var itemvm = menuItems.Select(item => new MenuCategoryVM
        {
            ItemId = item.ItemId,
            ItemName = item.ItemName,
            UnitId = item.UnitId,
            CategoryId = item.CategoryId,
            ItemtypeId = item.ItemtypeId,
            Rate = item.Rate,
            Quantity = item.Quantity,
            IsAvailable = (bool)item.IsAvailable,
            TaxDefault = item.TaxDefault,
            TaxPercentage = item.TaxPercentage,
            ShortCode = item.ShortCode,
            Description = item.Description,
            ItemPhoto = item.CategoryPhoto,
            IsFavourite = _menuRepository.IsItemFavourite(item.ItemId, user.UserId) ? true : false,
        }).ToList();

        return itemvm;
    }

    public bool AddToFavouriteItem(Favourite favouriteItem)
    {
        var existingItem = _context.Favourites.FirstOrDefault(m => m.ItemId == favouriteItem.ItemId && m.UserId == favouriteItem.UserId);
        if (existingItem == null)
        {
            _context.Favourites.Add(favouriteItem);
            Save();
            return true;
        }
        else
        {
            _context.Favourites.Remove(existingItem);
            Save();
            return false;
        }
    }

    public bool DeleteWaitingToken(int waitingId)
    {
        var waitingList = _context.WaitingLists.FirstOrDefault(w => w.WaitingListId == waitingId);
        if (waitingList != null)
        {
            waitingList.IsDeleted = true;
            Save();
            return true;
        }
        return false;
    }

    public bool EditCustomer(Customer customerdata)
    {
        Save();
        return true;
    }

    public void UpdateWaitingList(WaitingListVM waitingList)
    {
        var existingItem = _context.WaitingLists.FirstOrDefault(m => m.WaitingListId == waitingList.WaitingListId);
        if (existingItem != null)
        {
            existingItem.Name = waitingList.Name;
            existingItem.NoOfPerson = waitingList.NoOfPerson;
            existingItem.Phone = waitingList.Phone;
            existingItem.Email = waitingList.Email;
            existingItem.SectionId = (int)waitingList.SectionId;
            Save();
        }
    }

    public Order GetOrderItemById(int orderId)
    {
        var orderItem = _context.Orders
            .Include(oi => oi.OrderItems)
            .ThenInclude(oi => oi.OrderModifiers)
            .FirstOrDefault(oi => oi.OrderId == orderId);

        return orderItem;
    }

    public void DeleteOrderItem(int OrderItemId)
    {
        var orderItem = _context.OrderItems.FirstOrDefault(x => x.OrderItemId == OrderItemId);
        if (orderItem != null)
        {
            _context.OrderItems.Remove(orderItem);
            Save();
        }
    }

    public void DeleteOrderModifier(int OrderModifierId)
    {
        var orderModifier = _context.OrderModifiers.FirstOrDefault(x => x.OrderModifierId == OrderModifierId);
        if (orderModifier != null)
        {
            _context.OrderModifiers.Remove(orderModifier);
            Save();
        }
    }

    public void DeleteOrderTax(int OrderTaxId)
    {
        var orderTax = _context.OrderTaxes.FirstOrDefault(x => x.OrderTaxId == OrderTaxId);
        if (orderTax != null)
        {
            _context.OrderTaxes.Remove(orderTax);
            Save();
        }
    }

    public void UpdateCustomerTableStatus(int tableId)
    {
        var customerTable = _context.CustomerTables.FirstOrDefault(x => x.TableId == tableId);
        if (customerTable != null)
        {
            customerTable.IsActive = false;
            _context.CustomerTables.Update(customerTable);
            Save();
        }
    }

    public void AddReview(Review review)
    {
        _context.Reviews.Add(review);
        Save();
    }

    public void AddOrderTable(OrderTable orderTable)
    {
        _context.OrderTables.Add(orderTable);
        Save();
    }
    public void Save()
    {
        _context.SaveChanges();
    }
}