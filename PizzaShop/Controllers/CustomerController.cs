using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using DAL.Interfaces;
using DAL.Repository;
using ClosedXML.Excel;
using Entities.Models;
using Entities.ViewModel;
using DinkToPdf;
using DinkToPdf.Contracts;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using SelectPdf;
using BLL.Interfaces;

namespace PizzaShop.Controllers;


public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }


    // [Authorize(Policy = "CustomersViewPolicy")]
    public IActionResult Customer(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;
        ViewBag.SortBy = filterOptions.SortBy;
        ViewBag.IsAsc = filterOptions.IsAsc;



        var ordervm = _customerService.GetFilteredCustomers(filterOptions, orderStatus, filterdate, startDate, endDate);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_CustomerPV", ordervm);
        }
        return View(ordervm);
    }



    public IActionResult CustomerDetails(int customerId)
    {
        var customer = _customerService.GetCustomerByCustomerId(customerId);
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        return PartialView("_CustomerDetailsPV", customer);
    }

    public IActionResult ExportToExcel(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        filterOptions.PageSize = _customerService.GetAllCustomers().Count();
        Console.WriteLine(filterOptions.Page);
        var customers = _customerService.GetFilteredCustomers(filterOptions, orderStatus, filterdate, startDate, endDate);
        var totalcustomer = customers.Count;

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Orders");

            // Add Logo
            // var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            // if (System.IO.File.Exists(logoPath))
            // {
            //     var picture = worksheet.AddPicture(logoPath).MoveTo(worksheet.Cell("A1")).Scale(0.5);
            // }

            var statusLabelCell = worksheet.Range("A2:B3").Merge();
            statusLabelCell.Value = "Account:";
            statusLabelCell.Style.Font.Bold = true;
            statusLabelCell.Style.Font.FontSize = 12;
            statusLabelCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);// Custom Sapphire Blue background
            statusLabelCell.Style.Font.FontColor = XLColor.White; // White text
            statusLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            statusLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            statusLabelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var allStatusCell = worksheet.Range("C2:F3").Merge();
            allStatusCell.Value = "";
            allStatusCell.Style.Font.Bold = true;
            allStatusCell.Style.Font.FontSize = 12;
            allStatusCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            allStatusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            allStatusCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var searchLabelCell = worksheet.Range("H2:I3").Merge();
            searchLabelCell.Value = "Search Text:";
            searchLabelCell.Style.Font.Bold = true;
            searchLabelCell.Style.Font.FontSize = 12;
            searchLabelCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);// Custom Sapphire Blue background
            searchLabelCell.Style.Font.FontColor = XLColor.White; // White text
            searchLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            searchLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            searchLabelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var allSerachsCell = worksheet.Range("J2:K3").Merge();
            allSerachsCell.Value = filterOptions.Search;
            allSerachsCell.Style.Font.Bold = true;
            allSerachsCell.Style.Font.FontSize = 12;
            allSerachsCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            allSerachsCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            allSerachsCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var dateLabelCell = worksheet.Range("A5:B6").Merge();
            dateLabelCell.Value = "Date:";
            dateLabelCell.Style.Font.Bold = true;
            dateLabelCell.Style.Font.FontSize = 12;
            dateLabelCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);// Custom Sapphire Blue background
            dateLabelCell.Style.Font.FontColor = XLColor.White; // White text
            dateLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dateLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            dateLabelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var alldateCell = worksheet.Range("C5:F6").Merge();
            alldateCell.Value = filterdate ?? "All Date";
            alldateCell.Style.Font.Bold = true;
            alldateCell.Style.Font.FontSize = 12;
            alldateCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            alldateCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            alldateCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var noOfRecordLabelCell = worksheet.Range("H5:I6").Merge();
            noOfRecordLabelCell.Value = "No. Of Records:";
            noOfRecordLabelCell.Style.Font.Bold = true;
            noOfRecordLabelCell.Style.Font.FontSize = 12;
            noOfRecordLabelCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);// Custom Sapphire Blue background
            noOfRecordLabelCell.Style.Font.FontColor = XLColor.White; // White text
            noOfRecordLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            noOfRecordLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            noOfRecordLabelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var allnoOfRecordCell = worksheet.Range("J5:K6").Merge();
            allnoOfRecordCell.Value = totalcustomer;
            allnoOfRecordCell.Style.Font.Bold = true;
            allnoOfRecordCell.Style.Font.FontSize = 12;
            allnoOfRecordCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            allnoOfRecordCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            allnoOfRecordCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assest", "logos", "pizzashop_logo.png");

            var picture = worksheet.AddPicture(logoPath)
                .MoveTo(worksheet.Cell("M1"))
                .Scale(0.5); // Adjust the size



            // Header Styling
            var headerRow = worksheet.Range("A11:Q11");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Column Headers
            worksheet.Range("A11:B11").Merge().Value = "ID";
            worksheet.Range("C11:E11").Merge().Value = "Name";
            worksheet.Range("F11:I11").Merge().Value = "Email";
            worksheet.Range("J11:L11").Merge().Value = "Date";
            worksheet.Range("M11:O11").Merge().Value = "Mobile Number";
            worksheet.Range("P11:Q11").Merge().Value = "Total Order";

            // Apply Borders to Headers
            headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = 12; // Start writing data from row 12
            foreach (var customer in customers)
            {
                worksheet.Range("A" + row + ":B" + row).Merge().Value = customer.CustomerId;
                worksheet.Range("C" + row + ":E" + row).Merge().Value = customer.Name;
                worksheet.Range("F" + row + ":I" + row).Merge().Value = customer.Email;
                worksheet.Range("J" + row + ":L" + row).Merge().Value = customer.Date?.ToString("yyyy-MM-dd") ?? "N/A";
                worksheet.Range("M" + row + ":O" + row).Merge().Value = customer.Phone;
                worksheet.Range("P" + row + ":Q" + row).Merge().Value = customer.TotalOrder;

                // Apply styles to data rows
                var dataRow = worksheet.Range("A" + row + ":P" + row);
                dataRow.Style.Font.FontName = "Arial";
                dataRow.Style.Font.FontSize = 12;
                dataRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            // Adjust column widths
            worksheet.Columns().AdjustToContents();


            // Save to memory stream
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
            }
        }
    }






    private int TryParseInt(object value)
    {
        return int.TryParse(value?.ToString(), out int result) ? result : 0;
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}