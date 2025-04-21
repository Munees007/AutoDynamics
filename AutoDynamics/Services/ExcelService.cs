using AutoDynamics.Shared.Modals;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf.qrcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Services
{
    class ExcelService : IExcelService
    {
        public byte[] GenerateExcel(BillDetails[] bills,string details)
        {
            using var workBook = new XLWorkbook();
            var ws = workBook.Worksheets.Add(details);
            
            ws.Cell(1, 1).Value = "DATE";
            ws.Cell(1, 2).Value = "HSN CODE";
            ws.Cell(1, 3).Value = "B. NO";
            ws.Cell(1, 4).Value = "CUSTOMER NAME";
            ws.Cell(1, 5).Value = "VEHICLE NO";
            ws.Cell(1, 6).Value = "VEHICLE NAME";
            ws.Cell(1, 7).Value = "GST NO";
            ws.Cell(1, 8).Value = "TAX %";
            ws.Cell(1, 9).Value = "TAX VALUE";
            ws.Cell(1, 10).Value ="CGST";
            ws.Cell(1, 11).Value ="SGST";
            ws.Cell(1, 12).Value ="ROUND";
            ws.Cell(1, 13).Value = "TOTAL";
            var headerRow = ws.Row(1);
            headerRow.Style.Font.FontColor = XLColor.Red;
            int excelRow = 2; // Start writing from Excel row 2

            for (int row = 0; row < bills.Length; row++)
            {
                var bill = bills[row].Bill;
                var customer = bills[row].customer;
                string billID = bill.Branch == "Sivakasi"
                    ? "SFR" + bill.BillNo.ToString().PadLeft(4, '0')
                    : "BPR" + bill.BillNo.ToString().PadLeft(4, '0');

                // --- SERVICE ITEMS (Tax 18%) ---
                var serviceItems = bill.BillItems
                    .Where(i => i.ItemType == "SERVICE")
                    .ToList();

                if (serviceItems.Any())
                {
                    decimal serviceTotal = serviceItems.Sum(i => i.TotalPrice);
                    var taxableValue = Math.Round(serviceTotal / 1.18m, 2);
                    var cgst = Math.Round(taxableValue * 0.09m, 2);
                    var sgst = Math.Round(taxableValue * 0.09m, 2);
                    var roundingDiffRaw = serviceTotal - (taxableValue + cgst + sgst);
                    var roundingDiff = roundingDiffRaw > 0 ? Math.Round(roundingDiffRaw, 2) : 0m;

                    ws.Cell(excelRow, 1).Value = bill.BillDate.ToString("dd-MM-yyyy");
                    ws.Cell(excelRow, 2).Value = 998729; // Fixed SAC Code for service
                    ws.Cell(excelRow, 3).Value = billID;
                    ws.Cell(excelRow, 4).Value = customer.Name;
                    ws.Cell(excelRow, 5).Value = bill.VehicleNo;
                    ws.Cell(excelRow, 6).Value = bill.Vehicle.ModelName;
                    ws.Cell(excelRow, 7).Value = customer.GSTIN;
                    ws.Cell(excelRow, 8).Value = 18;
                    ws.Cell(excelRow, 9).Value = taxableValue;
                    ws.Cell(excelRow, 10).Value = cgst;
                    ws.Cell(excelRow, 11).Value = sgst;
                    ws.Cell(excelRow, 12).Value = roundingDiff;
                    ws.Cell(excelRow, 13).Value = serviceTotal;
                    excelRow++;
                }

                // --- PRODUCT ITEMS with TaxRate.TAX_28 ---
                var productItems = bill.BillItems
                    .Where(i => i.ItemType == "PRODUCT" && i.TaxRate == TaxRate.TAX_28)
                    .GroupBy(i => i.HSNCode); // Group by HSN if multiple

                foreach (var hsnGroup in productItems)
                {
                    decimal productTotal = hsnGroup.Sum(i => i.TotalPrice);
                    var taxableValue = Math.Round(productTotal / 1.28m, 2);
                    var cgst = Math.Round(taxableValue * 0.14m, 2);
                    var sgst = Math.Round(taxableValue * 0.14m, 2);
                    var roundingDiffRaw = productTotal - (taxableValue + cgst + sgst);
                    var roundingDiff = roundingDiffRaw > 0 ? Math.Round(roundingDiffRaw, 2) : 0m;

                    string hsnCode = hsnGroup.Key;

                    ws.Cell(excelRow, 1).Value = bill.BillDate.ToString("dd-MM-yyyy");
                    ws.Cell(excelRow, 2).Value = hsnCode; // HSN code from item
                    ws.Cell(excelRow, 3).Value = billID;
                    ws.Cell(excelRow, 4).Value = customer.Name;
                    ws.Cell(excelRow, 5).Value = bill.VehicleNo;
                    ws.Cell(excelRow, 6).Value = bill.Vehicle.ModelName;
                    ws.Cell(excelRow, 7).Value = customer.GSTIN;
                    ws.Cell(excelRow, 8).Value = 28;
                    ws.Cell(excelRow, 9).Value = taxableValue;
                    ws.Cell(excelRow, 10).Value = cgst;
                    ws.Cell(excelRow, 11).Value = sgst;
                    ws.Cell(excelRow, 12).Value = roundingDiff;
                    ws.Cell(excelRow, 13).Value = productTotal;
                    excelRow++;
                }
            }
            using var ms = new MemoryStream();
            workBook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
