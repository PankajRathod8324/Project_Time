using Entities.Models;
using DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Entities.ViewModel;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using BLL.Interfaces;

namespace BLL.Services;

public class AccountManagerOrderAppService : IAccountManagerOrderAppService
{
    private readonly ICustomerRepository _customerRepository;

    private readonly IOrderRepository _orderRepository;

    private readonly IMenuRepository _menuRepository;

    private readonly ITableAndSectionRepository _tableRepository;

    private readonly ITaxRepository _taxRepository;

    private readonly IAccountManagerOrderAppRepository _accountmanagerorderapprepository;

    private readonly IUserRepository _userRepository;



    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountManagerOrderAppService(ICustomerRepository customerRepository, IOrderRepository orderRepository, ITableAndSectionRepository tableRepository, IAccountManagerOrderAppRepository accountmanagerorderapprepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IMenuRepository menuRepository, ITaxRepository taxRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _accountmanagerorderapprepository = accountmanagerorderapprepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _menuRepository = menuRepository;
        _taxRepository = taxRepository;
    }



    public List<OrderSectionVM> GetOrderSection()
    {
        var sections = _accountmanagerorderapprepository.GetOrderSection();

        return sections;
    }

    public int GetCustomerIDByName(string email)
    {
        return _accountmanagerorderapprepository.GetCustomerIDByName(email);
    }

    public List<WaitingListVM> GetAllWaitingListCustomer(int sectionId)
    {
        return _accountmanagerorderapprepository.GetAllWaitingListCustomer(sectionId);
    }

    public int GetOrderIdByCustomerId(int customerId)
    {
        var orderid = _customerRepository.GetOrderIdByCustomerId(customerId);
        return (int)orderid;
    }

    public int GetOrderIdByCustomerIdActiveOrder(int customerId)
    {
        var orderid = _customerRepository.GetOrderIdByCustomerIdActiveOrder(customerId);
        return (int)orderid;
    }

    public bool IsCustomerAssociatedWithOrder(int customerId, int orderId)
    {
        return _accountmanagerorderapprepository.IsCustomerAssociatedWithOrder(customerId, orderId);
    }

    public List<CustomerTableVM> GetTablesByCustomerId(int customerId)
    {
        var customertable = _accountmanagerorderapprepository.GetTablesByCustomerId(customerId).Select(t => new CustomerTableVM
        {
            SectionName = _tableRepository.GetSectionNameByTableId(t.TableId ?? 0),
            TableName = _tableRepository.GetTableNameByTableId(t.TableId ?? 0)
        }).ToList();


        return customertable;
    }

    public List<CustomerTableVM> GetTablesByActiveCustomerId(int customerId)
    {
        var customertable = _accountmanagerorderapprepository.GetTablesByActiveCustomerId(customerId).Select(t => new CustomerTableVM
        {
            SectionName = _tableRepository.GetSectionNameByTableId(t.TableId ?? 0),
            TableName = _tableRepository.GetTableNameByTableId(t.TableId ?? 0)
        }).ToList();


        return customertable;
    }

    public WaitingListVM GetWaitingData(int waitingId)
    {
        return _accountmanagerorderapprepository.GetWaitingData(waitingId);
    }

    public WaitingListVM GetWaitingDataByEmail(string email)
    {
        return _accountmanagerorderapprepository.GetWaitingDataByEmail(email);
    }

    public CustomerVM GetCustomerDetails(int customerId)
    {
        var personaldata = _accountmanagerorderapprepository.GetCustomer(customerId);
        var finaldata = new CustomerVM
        {
            CustomerId = personaldata.CustomerId,
            Name = personaldata.Name,
            Email = personaldata.Email,
            Phone = personaldata.Phone,
            NoOfPerson = personaldata.Noofperson ?? 0
        };
        return finaldata;
    }

    public Customer IsCustomer(string email)
    {
        return _accountmanagerorderapprepository.IsCustomer(email);
    }

    public bool AddOrUpdateCustomer([FromBody] JsonObject customerdata)
    {

        // Extract customer details from JSON
        string email = customerdata["email"]?.ToString() ?? string.Empty;
        string name = customerdata["Name"]?.ToString() ?? string.Empty;
        string phone = customerdata["Phone"]?.ToString() ?? string.Empty;
        int noOfPerson = TryParseInt(customerdata["noOfPerson"] ?? 0);
        List<Table> selectedTables = new();

        if (customerdata.ContainsKey("SelectedTable") && customerdata["SelectedTable"] != null)
        {
            var selectedTableJson = customerdata["SelectedTable"]?.ToString();
            if (!string.IsNullOrEmpty(selectedTableJson))
            {
                selectedTables = JsonConvert.DeserializeObject<List<Table>>(selectedTableJson) ?? new List<Table>();
            }
        }

        // Check if the customer already exists
        var existingCustomer = _accountmanagerorderapprepository.IsCustomer(email);
        var orderId = _customerRepository.GetOrderIdByCustomerIdActiveOrder(existingCustomer.CustomerId);
        if (orderId != 0)
        {
            return false;
        }

        if (existingCustomer != null)
        {
            // Customer exists -> Update NoOfPerson
            existingCustomer.Noofperson = noOfPerson;
            _customerRepository.UpdateCustomer(existingCustomer);
            if (selectedTables != null)
            {
                // Add selected tables to the CustomerTable relation
                foreach (var table in selectedTables)
                {
                    var customerTable = new CustomerTable
                    {
                        CustomerId = existingCustomer.CustomerId,
                        TableId = table.TableId
                    };
                    var tablestatus = new Table
                    {
                        TableId = table.TableId,
                        StatusId = 3,
                        OccupiedTime = DateTime.UtcNow
                    };
                    _tableRepository.UpdateTable(tablestatus);
                    _customerRepository.AddCustomerTable(customerTable);
                    // if customer also in waitinglist then delete that customer from waitinglist
                    var waitingList = _accountmanagerorderapprepository.IsInWaitingList(email);
                    if (waitingList != null)
                    {
                        _accountmanagerorderapprepository.DeleteCustomerFromWaitingList(email);
                    }
                }
            }
        }
        else
        {
            // Customer does not exist -> Insert new customer
            var newCustomer = new Customer
            {
                Email = email,
                Name = name ?? string.Empty,
                Phone = phone ?? string.Empty,
                Noofperson = noOfPerson
            };
            _customerRepository.AddCustomer(newCustomer);
            _accountmanagerorderapprepository.DeleteCustomerFromWaitingList(email);

            if (selectedTables != null)
            {
                // Add selected tables to the CustomerTable relation
                foreach (var table in selectedTables)
                {
                    var customerTable = new CustomerTable
                    {
                        CustomerId = newCustomer.CustomerId,
                        TableId = table.TableId
                    };
                    var tablestatus = new Table
                    {
                        TableId = table.TableId,
                        StatusId = 3,
                        OccupiedTime = DateTime.UtcNow
                    };
                    _tableRepository.UpdateTable(tablestatus);
                    _customerRepository.AddCustomerTable(customerTable);
                }
            }
        }


        return true;

    }

    public bool AddInWaitingList([FromBody] JsonObject customerdata)
    {

        // Extract customer details from JSON
        string email = customerdata["email"]?.ToString() ?? string.Empty;
        string name = (customerdata["Name"]?.ToString()) ?? string.Empty;
        string phone = customerdata["Phone"]?.ToString() ?? string.Empty;
        int noOfPerson = TryParseInt(customerdata["noOfPerson"] ?? 0);
        int sectionId = TryParseInt(customerdata["SectionId"] ?? 0);
        var alreadyhaveToken = _accountmanagerorderapprepository.IsInWaitingList(email);
        if(alreadyhaveToken != null)
        {
            return false;
        }
        else
        {
            var newCustomer = new WaitingList
            {
                Email = email,
                Name = name,
                Phone = phone,
                NoOfPerson = noOfPerson,
                SectionId = sectionId
            };
            _accountmanagerorderapprepository.AddInWaitingList(newCustomer);

            return true;
        }

    }

    public WaitingListVM GetWaitingTokenById(int waitingId)
    {
        var waitingList = _accountmanagerorderapprepository.GetWaitingData(waitingId);
        var finaldata = new WaitingListVM
        {
            WaitingListId = waitingList.WaitingListId,
            Name = waitingList.Name,
            Email = waitingList.Email,
            Phone = waitingList.Phone,
            NoOfPerson = waitingList.NoOfPerson,
            SectionId = waitingList.SectionId,
            SectionName = _tableRepository.GetSectionById(waitingList.SectionId ?? 0).SectionName,
            CreatedAt = waitingList.CreatedAt
        };
        return finaldata;
    }

    public bool AddToFavouriteItem(int ItemId)
    {
        string useremail = _httpContextAccessor.HttpContext?.Request?.Cookies?["Email"] ?? string.Empty;
        var user = _userRepository.GetUserByEmail(useremail);
        var favouriteItem = new Favourite
        {
            ItemId = ItemId,
            UserId = user.UserId,
        };

        _accountmanagerorderapprepository.AddToFavouriteItem(favouriteItem);
        return true;
    }

    public List<MenuCategoryVM> GetFavouriteItem()
    {
        return _accountmanagerorderapprepository.GetFavouriteItems();
    }

    public List<OrderVM> GetOrderCategoryItem(int categoryId, string? status)
    {
        var orderCategoryItems = _accountmanagerorderapprepository.GetOrderCategoryItem(categoryId, status);
        return orderCategoryItems;
    }

    public bool UpdateOrderStatus(List<PrepareItemVM> prepare)
    {
        _accountmanagerorderapprepository.PrepareItem(prepare);

        return true; // Return true if the operation is successful
    }

    public List<TaxVM> GetOrderTaxesByOrderId(int OrderId)
    {
        var ordertax = _accountmanagerorderapprepository.GetOrderTaxesByOrderId(OrderId);
        var finaltaxdata = ordertax.Select(tax => new TaxVM
        {
            TaxId = tax.TaxId ?? 0,
            TaxAmount = tax.TaxRate ?? (decimal)0.00,
            TaxName = _taxRepository.GetTaxNameById(tax.TaxId ?? 0),
            TaxTypeId = _taxRepository.GetTexTypeIdByTaxId(tax.TaxId ?? 0),
        }).ToList();
        return finaltaxdata;
    }

    public List<TaxVM> GetOrderTaxesDefaultByOrderId(int OrderId)
    {
        var ordertax = _accountmanagerorderapprepository.GetOrderTaxesDefaultByOrderId(OrderId);
        var finaltaxdata = ordertax.Select(tax => new TaxVM
        {
            TaxId = tax.TaxId ?? 0,
            TaxAmount = tax.TaxRate ?? (decimal)0.00,
            TaxName = _taxRepository.GetTaxNameById(tax.TaxId ?? 0),
            TaxTypeId = _taxRepository.GetTexTypeIdByTaxId(tax.TaxId ?? 0),
        }).ToList();
        return finaltaxdata;
    }

    public bool DeleteWaitingToken(int waitingId)
    {
        var waitingList = _accountmanagerorderapprepository.GetWaitingData(waitingId);
        if (waitingList != null)
        {
            _accountmanagerorderapprepository.DeleteWaitingToken(waitingId);
            return true;
        }
        return false;
    }

    public bool AssignTable(int waitingId, int tableId)
    {
        var waitingList = _accountmanagerorderapprepository.GetWaitingData(waitingId);
        var IsCustomer = _accountmanagerorderapprepository.IsCustomer(waitingList.Email);
        if (IsCustomer != null)
        {
            //check if the customer has alreay order which is running
            var orderId = _customerRepository.GetOrderIdByCustomerIdActiveOrder(IsCustomer.CustomerId);
            if (orderId != 0)
            {
                return false;
            }
            IsCustomer.Noofperson = waitingList.NoOfPerson;
            _customerRepository.UpdateCustomer(IsCustomer);

            var customerTable = new CustomerTable
            {
                CustomerId = IsCustomer.CustomerId,
                TableId = tableId
            };
            var tablestatus = new Table
            {
                TableId = tableId,
                StatusId = 3,
                OccupiedTime = DateTime.UtcNow
            };
            _tableRepository.UpdateTable(tablestatus);
            _customerRepository.AddCustomerTable(customerTable);
            _accountmanagerorderapprepository.DeleteCustomerFromWaitingList(waitingList.Email);

        }
        else
        {
            var customer = new Customer
            {
                Email = waitingList.Email,
                Name = waitingList.Name,
                Phone = waitingList.Phone,
                Noofperson = waitingList.NoOfPerson
            };
            _customerRepository.AddCustomer(customer);
            _accountmanagerorderapprepository.DeleteCustomerFromWaitingList(waitingList.Email);

            var customerTable = new CustomerTable
            {
                CustomerId = customer.CustomerId,
                TableId = tableId
            };
            var tablestatus = new Table
            {
                TableId = tableId,
                StatusId = 3,
                OccupiedTime = DateTime.UtcNow
            };
            _tableRepository.UpdateTable(tablestatus);
            _customerRepository.AddCustomerTable(customerTable);
        }

        return true;
    }

    public MenuCategoryVM GetItemModifier(int ItemId)
    {
        var item = _menuRepository.GetItemById(ItemId);
        var itemmodifiers = _menuRepository.GetItemModifier(ItemId);
        return new MenuCategoryVM
        {
            ItemId = ItemId,
            ItemName = _menuRepository.GetItemNameById(ItemId),
            Rate = _menuRepository.GetItemById(item.ItemId).Rate,
            ModifierGroupIds = itemmodifiers.Select(m => new ItemModifierVM
            {
                ModifierGroupId = m.ModifierGroupId,
                ModifierGroupName = _menuRepository.GetModifierGroupNameById(m.ModifierGroupId),
                MaxSelection = m.MaxSelection,
                MinSelection = m.MinSelection,
                MenuModifiers = _menuRepository.GetModifiersByModifierGroupId(m.ModifierGroupId).Select(mod => new ModifierVM
                {
                    ModifierId = mod.ModifierId,
                    ModifierName = mod.ModifierName,
                    ModifierRate = mod.ModifierRate ?? (decimal)0.00,
                }).ToList()
            }).ToList()
        };


    }

    public List<MenuCategoryVM> SetOrderItemData(List<MenuCategoryVM> ItemDetail)
    {
        var finalitemdata = ItemDetail.Select(item => new MenuCategoryVM
        {
            ItemId = item.ItemId,
            ItemName = _menuRepository.GetItemById(item.ItemId).ItemName,
            CategoryId = _menuRepository.GetItemById(item.ItemId).CategoryId,
            Rate = _menuRepository.GetItemById(item.ItemId).Rate,
            TaxPercentage = _menuRepository.GetItemById(item.ItemId).TaxPercentage,
            Quantity = 1,
            UId = item.UId,
            MenuItemModifier = item.MenuItemModifier.Select(mod => new MenuModifierGroupVM
            {
                ModifierId = mod.ModifierId,
                ModifierName = _menuRepository.GetModifierNameById(mod.ModifierId),
                ModifierRate = _menuRepository.GetModifierById(mod.ModifierId).ModifierRate,
            }).ToList(),

        }).ToList();
        return finalitemdata;
    }

    public List<MenuCategoryVM> SetOrderItemDataWithId(int CustomerId)
    {
        var OrderId = _customerRepository.GetOrderIdByCustomerIdActiveOrder(CustomerId);
        if (OrderId == 0)
        {
            return new List<MenuCategoryVM>();
        }
        var orderitem = _orderRepository.GetOrderItemsByOrderId(OrderId);
        var ordertax = _orderRepository.GetOrderTaxesByOrderId(OrderId);
        var ordertable = _orderRepository.GetOrderTablesByOrderId(OrderId);

        var finaldata = new List<MenuCategoryVM>();
        foreach (var item in orderitem)
        {
            var itemdata = new MenuCategoryVM
            {
                ItemId = item.ItemId ?? 0,
                ItemName = _menuRepository.GetItemById(item.ItemId ?? 0).ItemName,
                Rate = _menuRepository.GetItemById(item.ItemId ?? 0).Rate,
                ItemInstruction = item.ItemInstructions ?? string.Empty,
                Quantity = item.Quantity,
                UId = item.Uid ?? string.Empty,
                MenuItemModifier = _orderRepository.GetOrderModifiersByOrderItemIdAndItemId(item.OrderItemId, item.ItemId ?? 0).Select(mod => new MenuModifierGroupVM
                {
                    ModifierId = mod.ModifierId ?? 0,
                    ModifierName = _menuRepository.GetModifierNameById(mod.ModifierId ?? 0),
                    ModifierRate = _menuRepository.GetModifierById(mod.ModifierId ?? 0).ModifierRate,
                }).ToList(),
            };
            finaldata.Add(itemdata);
        }
        return finaldata;
    }

    public bool EditCustomer(CustomerVM customerdata)
    {
        var existingCustomer = _accountmanagerorderapprepository.GetCustomer(customerdata.CustomerId);
        if (existingCustomer != null)
        {
            existingCustomer.Name = customerdata.Name;
            existingCustomer.Email = customerdata.Email;
            existingCustomer.Noofperson = customerdata.NoOfPerson;
            existingCustomer.Phone = customerdata.Phone;
        }


        return _accountmanagerorderapprepository.EditCustomer(existingCustomer ?? new Customer());
    }

    public bool SaveOrUpdateOrder(List<MenuCategoryVM> orderDetailsArray)
    {
        var existorder = _orderRepository.GetOrderById(orderDetailsArray[0].OrderId);

        if (existorder != null)
        {

            // Update existing order
            existorder.CustomerId = orderDetailsArray[0].CustomerId;
            existorder.NoOfPerson = _customerRepository.GetNoOfPersons(orderDetailsArray[0].CustomerId ?? 0);
            existorder.IsSgstInclude = orderDetailsArray[0].IsSgstIncluded;
            existorder.SgstAmt = orderDetailsArray[0].SgstAmt;
            existorder.OrderInstructions = orderDetailsArray[0].OrderComment;
            existorder.SubTotal = orderDetailsArray[0].SubTotal;
            existorder.TotalAmount = orderDetailsArray[0].TotalAmount;
            existorder.OrderStatusId = 6;
            existorder.OrderType = orderDetailsArray[0].OrderType ?? "DineIn";
            _orderRepository.UpdateOrder(existorder);

            var orderId = existorder.OrderId;
            //now there are list of orderitems which may repeat like it is new list of item in that if the item is updated then update it optherwise append new ones and if deleted and delete the existing one


            var existingOrderItems = _orderRepository.GetOrderItemsByOrderId(orderId);

            // Delete order items that are not in the new list

            foreach (var existingItem in existingOrderItems)
            {
                var itemInNewList = orderDetailsArray.FirstOrDefault(i => i.UId == existingItem.Uid);
                var modifierNewList = _orderRepository.GetOrderModifiersByOrderItemIdAndItemId(existingItem.OrderItemId, existingItem.ItemId ?? 0);


                if (itemInNewList == null)
                {
                    foreach (var modifier in modifierNewList)
                    {
                        // Delete order modifiers that are not in the new list
                        var modifierInNewList = orderDetailsArray.FirstOrDefault(i => i.ItemId == existingItem.ItemId)?.MenuItemModifier.FirstOrDefault(m => m.ModifierId == modifier.ModifierId);
                        if (modifierInNewList == null)
                        {
                            _accountmanagerorderapprepository.DeleteOrderModifier(modifier.OrderModifierId);
                        }
                    }
                    _accountmanagerorderapprepository.DeleteOrderItem(existingItem.OrderItemId);
                }


            }



            // Update existing order items
            foreach (var item in orderDetailsArray)
            {
                var newitemorderitemid = 0;
                var existingOrderItem = _orderRepository.GetOrderItemsByOrderId(orderId).FirstOrDefault(oi => oi.Uid == item.UId);
                if (existingOrderItem != null)
                {
                    existingOrderItem.Quantity = item.Quantity;
                    existingOrderItem.Rate = item.Rate;
                    existingOrderItem.ItemInstructions = item.ItemInstruction;
                    _orderRepository.UpdateOrderItem(existingOrderItem);
                }
                else
                {
                    var orderItem = new OrderItem
                    {
                        ItemId = item.ItemId,
                        OrderId = orderId,
                        CategoryId = _menuRepository.GetItemById(item.ItemId).CategoryId,
                        Quantity = item.Quantity,
                        Readyitemquantity = 0,
                        Uid = item.UId,
                        Rate = item.Rate,
                        ItemInstructions = item.ItemInstruction,
                        Status = "In Progress",
                    };
                    _orderRepository.AddOrderItem(orderItem);
                    newitemorderitemid = orderItem.OrderItemId;
                }

                if (item.MenuItemModifier != null)
                {
                    foreach (var modifier in item.MenuItemModifier)
                    {
                        if (existingOrderItem != null)
                        {
                            var existingModifier = _orderRepository.GetOrderModifiersByOrderItemIdAndItemId(existingOrderItem.OrderItemId, item.ItemId).FirstOrDefault(m => m.ModifierId == modifier.ModifierId);
                            if (existingModifier != null)
                            {
                                existingModifier.Rate = _menuRepository.GetModifierPriceById(modifier.ModifierId);
                                _orderRepository.UpdateOrderModifier(existingModifier);
                            }
                        }


                        else
                        {
                            var orderModifier = new OrderModifier
                            {
                                ItemId = item.ItemId,
                                OrderItemId = existingOrderItem == null ? newitemorderitemid : existingOrderItem.OrderItemId,
                                ModifierId = modifier.ModifierId,
                                Uid = item.UId,
                                Rate = _menuRepository.GetModifierPriceById(modifier.ModifierId),
                            };
                            _orderRepository.AddOrderModifier(orderModifier);
                        }

                    }
                }
            }
            // Update existing order taxes
            var existingOrderTaxes = _orderRepository.GetOrderTaxesByOrderId(orderId);
            foreach (var tax in existingOrderTaxes)
            {
                if (orderDetailsArray[0].IsSgstIncluded == true)
                {
                    //if sgst tax is not included then do not add sgst tax in ordertax
                    existorder.SgstAmt = orderDetailsArray[0].SgstAmt;
                    _orderRepository.UpdateOrder(existorder);
                }
                else
                {
                    var existingTax = orderDetailsArray[0].OrderTax?.FirstOrDefault(t => t.TaxId == tax.TaxId);
                    if (existingTax != null)
                    {
                        tax.TaxRate = existingTax.TaxAmount;
                        _orderRepository.UpdateOrderTax(tax);
                    }
                    else
                    {
                        _accountmanagerorderapprepository.DeleteOrderTax(tax.OrderTaxId);
                    }
                    _orderRepository.UpdateOrderTax(tax);
                }
            }

            return true;

        }
        else
        {
            var newOrder = new Order
            {
                CustomerId = orderDetailsArray[0].CustomerId,
                NoOfPerson = _customerRepository.GetNoOfPersons(orderDetailsArray[0].CustomerId ?? 0),
                IsSgstInclude = orderDetailsArray[0].IsSgstIncluded,
                SgstAmt = orderDetailsArray[0].SgstAmt,
                OrderInstructions = orderDetailsArray[0].OrderComment,
                SubTotal = orderDetailsArray[0].SubTotal,
                TotalAmount = orderDetailsArray[0].TotalAmount,
                OrderStatusId = 6,
                OrderType = orderDetailsArray[0].OrderType ?? "DineIn",
                PlacedOn = DateTime.UtcNow,
            };
            _orderRepository.AddOrder(newOrder);
            // make to insert invoice no after order creation using orderid
            newOrder.InvoiceNo = gernerateInvoiceNo(newOrder.OrderId);
            _orderRepository.UpdateOrder(newOrder);

            var orderId = newOrder.OrderId;

            foreach (var item in orderDetailsArray)
            {
                var orderItem = new OrderItem
                {
                    ItemId = item.ItemId,
                    OrderId = orderId,
                    CategoryId = _menuRepository.GetItemById(item.ItemId).CategoryId,
                    Quantity = item.Quantity,
                    Readyitemquantity = 0,
                    Uid = item.UId,
                    Rate = item.Rate,
                    ItemInstructions = item.ItemInstruction,
                    Status = "In Progress",
                };
                _orderRepository.AddOrderItem(orderItem);

                if (item.MenuItemModifier != null)
                {
                    foreach (var modifier in item.MenuItemModifier)
                    {
                        var orderModifier = new OrderModifier
                        {
                            ItemId = item.ItemId,
                            OrderItemId = orderItem.OrderItemId,
                            ModifierId = modifier.ModifierId,
                            Uid = item.UId,
                            Rate = _menuRepository.GetModifierPriceById(modifier.ModifierId),
                        };
                        _orderRepository.AddOrderModifier(orderModifier);
                    }
                }
            }


            foreach (var tax in orderDetailsArray[0].OrderTax)
            {

                var orderTaxItem = new OrderTax
                {
                    OrderId = orderId,
                    TaxId = tax.TaxId,
                    TaxRate = tax.TaxAmount,
                };
                _orderRepository.AddOrderTax(orderTaxItem);

            }


            var table = _accountmanagerorderapprepository.GetTablesByActiveCustomerId(orderDetailsArray[0].CustomerId ?? 0);
            foreach (var tb in table)
            {
                var ordertable = new OrderTable
                {
                    OrderId = orderId,
                    TableId = tb.TableId,
                };
                _accountmanagerorderapprepository.AddOrderTable(ordertable);
            }

        }
        var tables = _accountmanagerorderapprepository.GetTablesByActiveCustomerId(orderDetailsArray[0].CustomerId ?? 0);

        foreach (var table in tables)
        {
            var selectedtable = _tableRepository.GetTableById(table.TableId ?? 0);
            selectedtable.StatusId = 4;
            _tableRepository.UpdateTable(selectedtable);
        }

        return true;
    }

    public bool CompleteOrder(int ordreid, int customerId, string paymentOption)
    {
        var order = _orderRepository.GetOrderById(ordreid);
        var orderitem = _orderRepository.GetOrderItemsByOrderId(ordreid);
        //check if any orderitem has status "Ready" then can't cancel the order
        foreach (var item in orderitem)
        {
            if (item.Status == "In Progress")
            {
                return false;
            }
        }

        if (order != null)
        {
            order.OrderStatusId = 4;
            order.OrderDuration = DateTime.UtcNow - order.CreatedAt;
            _orderRepository.UpdateOrder(order);
        }
        var tables = _accountmanagerorderapprepository.GetTablesByCustomerId(customerId);

        foreach (var table in tables)
        {
            if (table.IsActive == true)
            {
                var selectedtable = _tableRepository.GetTableById(table.TableId ?? 0);
                selectedtable.StatusId = 1;
                table.IsActive = false;
                _accountmanagerorderapprepository.UpdateCustomerTableStatus(selectedtable.TableId);
                _tableRepository.UpdateTable(selectedtable);
            }

        }

        var paymentmodeid = _orderRepository.GetPaymentModeIdByMode(paymentOption);
        var payment = new Payment
        {
            OrderId = ordreid,
            PaymentStatusId = 1,
            PaymentMode = paymentOption,
            Amount = order?.TotalAmount,
        };
        _orderRepository.AddPayment(payment);

        return true;
    }
    public bool CancelOrder(int ordreid, int customerId)
    {
        var order = _orderRepository.GetOrderById(ordreid);
        var orderitem = _orderRepository.GetOrderItemsByOrderId(ordreid);
        //check if any orderitem has status "Ready" then can't cancel the order
        foreach (var item in orderitem)
        {
            if (item.Status == "Ready")
            {
                return false;
            }
        }
        if (order != null)
        {
            order.OrderStatusId = 3;
            _orderRepository.UpdateOrder(order);
        }

        var tables = _accountmanagerorderapprepository.GetTablesByCustomerId(customerId);

        foreach (var table in tables)
        {
            if (table.IsActive == true)
            {
                var selectedtable = _tableRepository.GetTableById(table.TableId ?? 0);
                selectedtable.StatusId = 1;
                table.IsActive = false;
                _accountmanagerorderapprepository.UpdateCustomerTableStatus(selectedtable.TableId);
                _tableRepository.UpdateTable(selectedtable);
            }
        }

        return true;
    }

    public bool SubmitReview(Review review)
    {
        var order = _orderRepository.GetOrderById(review.OrderId ?? 0);
        if (order != null)
        {
            var newReview = new Review
            {
                OrderId = review.OrderId,
                CustomerId = review.CustomerId,
                Rating = (review.Food + review.Service + review.Ambience) / 3,
                Comment = review.Comment,
                Food = review.Food,
                Service = review.Service,
                Ambience = review.Ambience
            };
            _accountmanagerorderapprepository.AddReview(newReview);
            return true;
        }
        return false;
    }

    private string gernerateInvoiceNo(int OrderId)
    {
        return "#DOM2025" + (OrderId).ToString("D4");

    }
    private static int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }



}