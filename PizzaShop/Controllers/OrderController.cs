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
// using DinkToPdf.Contracts;
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


public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public OrderController(IOrderService orderService, IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
    {
        _orderService = orderService;
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }


    [Authorize(Policy = "OrderViewPolicy")]
    public IActionResult Order(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        filterOptions.Page ??= 1;
        filterOptions.PageSize = filterOptions.PageSize != 0 ? filterOptions.PageSize : 10; // Default page size

        ViewBag.PageSize = filterOptions.PageSize;
        ViewBag.SortBy = filterOptions.SortBy;
        ViewBag.IsAsc = filterOptions.IsAsc;

        var status = _orderService.GetAllOrderStatuses();
        ViewBag.OrderStatuses = status.Select(r => new SelectListItem
        {
            Value = r.OrderStatusId.ToString(),
            Text = r.OrderStatusName
        }).ToList();

        var ordervm = _orderService.GetFilteredOrders(filterOptions, orderStatus, filterdate, startDate, endDate);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_OrderPV", ordervm);
        }
        return View(ordervm);
    }

    public IActionResult ExportToExcel(UserFilterOptions filterOptions, string orderStatus, string filterdate, string startDate, string endDate)
    {
        filterOptions.PageSize = _orderService.GetAllOrders().Count();
        Console.WriteLine(filterOptions.Page);
        var orders = _orderService.GetFilteredOrders(filterOptions, orderStatus, filterdate, startDate, endDate);
        var totalorder = orders.Count;

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
            statusLabelCell.Value = "Status:";
            statusLabelCell.Style.Font.Bold = true;
            statusLabelCell.Style.Font.FontSize = 12;
            statusLabelCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);// Custom Sapphire Blue background
            statusLabelCell.Style.Font.FontColor = XLColor.White; // White text
            statusLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            statusLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            statusLabelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var allStatusCell = worksheet.Range("C2:F3").Merge();
            allStatusCell.Value = orderStatus ?? "All Status";
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
            allnoOfRecordCell.Value = totalorder;
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
            var headerRow = worksheet.Range("A11:P11");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 167);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Column Headers
            worksheet.Cell(11, 1).Value = "Order ID";
            worksheet.Range("B11:D11").Merge().Value = "Date";
            worksheet.Range("E11:G11").Merge().Value = "Customer";
            worksheet.Range("H11:J11").Merge().Value = "Status";
            worksheet.Range("K11:L11").Merge().Value = "Payment Mode";
            worksheet.Range("M11:N11").Merge().Value = "Rating";
            worksheet.Range("O11:P11").Merge().Value = "Total Amount";

            // Apply Borders to Headers
            headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = 12; // Start writing data from row 12
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.OrderId;
                worksheet.Range("B" + row + ":D" + row).Merge().Value = order.Date.ToString("yyyy-MM-dd");
                worksheet.Range("E" + row + ":G" + row).Merge().Value = order.CustomerId.HasValue ? _orderService.GetCustomerById(order.CustomerId.Value) : "No Customer";
                worksheet.Range("H" + row + ":J" + row).Merge().Value = order.OrderStatusId.HasValue ? _orderService.GetOrderStatusById(order.OrderStatusId.Value) : "No Order Status";
                worksheet.Range("K" + row + ":L" + row).Merge().Value = order.PaymentModeId.HasValue ? _orderService.GetPaymentModeById(order.PaymentModeId.Value) : "No Payment Mode";
                worksheet.Range("M" + row + ":N" + row).Merge().Value = order.ReviewId.HasValue ? _orderService.GetReviewById(order.ReviewId.Value) : 0;
                worksheet.Range("O" + row + ":P" + row).Merge().Value = order.TotalAmount;

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

    // public IActionResult ExportToPDF(int orderId)
    // {
    //     var order = _orderService.GetOrderById(orderId);
    //     if (order == null)
    //     {
    //         return NotFound("Order not found");
    //     }

    //     try
    //     {
    //         // Create a new MemoryStream without a using statement
    //         var memoryStream = new MemoryStream();

    //         // Initialize the PdfWriter and PdfDocument
    //         var writer = new PdfWriter(memoryStream);
    //         var pdf = new PdfDocument(writer);
    //         var document = new Document(pdf);

    //         // Add content to the PDF
    //         document.Add(new Paragraph("Order Details").SetFontSize(18));
    //         document.Add(new Paragraph($"Order ID: {order.OrderId}"));
    //         document.Add(new Paragraph($"Date: {order.Date:yyyy-MM-dd}"));
    //         document.Add(new Paragraph($"Status: {order.OrderStatus}"));
    //         document.Add(new Paragraph($"Total Amount: {order.TotalAmount:C}"));

    //         // Close the document to finalize the PDF
    //         document.Close();

    //         // Reset the memory stream position to the beginning
    //         memoryStream.Position = 0;

    //         // Return the PDF file; the framework will handle the stream disposal
    //         return File(memoryStream, "application/pdf", $"Order_{orderId}.pdf");
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest($"Error generating PDF: {ex.Message}");
    //     }
    // }

    public IActionResult GeneratePDF(int orderId)
    {
        var order = _orderService.GetOrderByOrderId(orderId);
        if (order == null)
        {
            TempData["Message"] = "Order not found!";
            return RedirectToAction("Dashboard");
        }

        // ✅ Get absolute URL for Bootstrap & images
        string baseUrl = $"{Request.Scheme}://{Request.Host}";
        Console.WriteLine("BaseURL" + baseUrl);

        // ✅ Render Razor View as HTML string
        string htmlContent = RenderRazorViewToString("OrderPDF", order);
        htmlContent = htmlContent.Replace("~/", baseUrl + "/");  // Convert relative paths to absolute

        if (string.IsNullOrEmpty(htmlContent))
        {
            return Content("Error: Razor view rendering returned null.");
        }

        // ✅ Configure PDF settings
        HtmlToPdf converter = new HtmlToPdf();
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.WebPageWidth = 1200;
        converter.Options.WebPageHeight = 0; // Auto height
        converter.Options.MarginLeft = 20;
        converter.Options.MarginRight = 20;

        // ✅ Convert HTML to PDF
        SelectPdf.PdfDocument pdf = converter.ConvertHtmlString(htmlContent, baseUrl);

        // ✅ Save & return PDF
        using (var memoryStream = new MemoryStream())
        {
            pdf.Save(memoryStream);
            pdf.Close();
            return File(memoryStream.ToArray(), "application/pdf", $"Order_{orderId}.pdf");
        }
    }

    private string RenderRazorViewToString(string viewName, object model)
    {
        ViewData.Model = model;

        using (var sw = new StringWriter())
        {
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"View '{viewName}' not found.");
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );

            viewResult.View.RenderAsync(viewContext).Wait();
            return sw.ToString();
        }
    }
    public IActionResult OrderDetails(int orderId)
    {
        var order = _orderService.GetOrderByOrderId(orderId);
        if (order == null)
        {
            return NotFound("Order not found");
        }

        return View(order);
    }

    public IActionResult OrderPDF(int orderId)
    {
        var order = _orderService.GetOrderByOrderId(orderId);
        if (order == null)
        {
            return NotFound("Order not found");
        }
        return View(order);
    }

    public IActionResult GetImage(string filename)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assest/logos", filename);
        if (!System.IO.File.Exists(path))
        {
            return NotFound(); // Return 404 if image not found
        }
        return PhysicalFile(path, "image/png");
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