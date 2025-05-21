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
                var serviceItems = bills[row].BillItems
                    .Where(i => i.ItemType == "SERVICE")
                    .ToList();

                if (serviceItems.Any())
                {
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalQuantity = 0;
                    var totalTaxableAmount = 0m;
                    var total = 0m;
                    foreach (var item in serviceItems)
                    {
                        var cgstRate =  9;
                        var sgstRate =9;
                        var cgstAmt = Math.Round(item.TaxableValue * (cgstRate / 100m), 2);
                        var sgstAmt = Math.Round(item.TaxableValue * (sgstRate / 100m), 2);

                        totalCGST += cgstAmt;
                        totalSGST += sgstAmt;
                        totalQuantity += item.Quantity;
                        totalTaxableAmount += item.TaxableValue;
                        total += item.UnitPrice * item.Quantity;
                    }

                    var computedTotal = totalTaxableAmount + totalCGST + totalSGST;

                    // ✅ Rounding difference
                    var roundingDiffRaw = total - computedTotal;
                    var roundingDiff = Math.Round(roundingDiffRaw, 2);

                    ws.Cell(excelRow, 1).Value = bill.BillDate.ToString("dd-MM-yyyy");
                    ws.Cell(excelRow, 2).Value = 998729; // Fixed SAC Code for service
                    ws.Cell(excelRow, 3).Value = billID;
                    ws.Cell(excelRow, 4).Value = customer.Name;
                    ws.Cell(excelRow, 5).Value = bill.VehicleNo;
                    ws.Cell(excelRow, 6).Value = bill.Vehicle.ModelName;
                    ws.Cell(excelRow, 7).Value = customer.GSTIN;
                    ws.Cell(excelRow, 8).Value = 18;
                    ws.Cell(excelRow, 9).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 9).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 10).Value = totalCGST;
                    ws.Cell(excelRow, 10).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 11).Value = totalSGST;
                    ws.Cell(excelRow, 11).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 12).Value = roundingDiff;
                    ws.Cell(excelRow, 13).Value = total;
                    excelRow++;
                }

                // --- PRODUCT ITEMS with TaxRate.TAX_28 ---
                var productItems = bills[row].BillItems
                    .Where(i => i.ItemType == "PRODUCT" && i.TaxRate == TaxRate.TAX_28)
                    .GroupBy(i => i.HSNCode); // Group by HSN if multiple

                foreach (var hsnGroup in productItems)
                {
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalQuantity = 0;
                    var totalTaxableAmount = 0m;
                    var total = 0m;
                    foreach (var item in hsnGroup)
                    {
                        var cgstRate = 14;
                        var sgstRate = 14;
                        var cgstAmt = Math.Round(item.TaxableValue * (cgstRate / 100m), 2);
                        var sgstAmt = Math.Round(item.TaxableValue * (sgstRate / 100m), 2);

                        totalCGST += cgstAmt;
                        totalSGST += sgstAmt;
                        totalQuantity += item.Quantity;
                        totalTaxableAmount += item.TaxableValue;
                        total += item.UnitPrice * item.Quantity;
                    }

                    var computedTotal = totalTaxableAmount + totalCGST + totalSGST;

                    // ✅ Rounding difference
                    var roundingDiffRaw = total - computedTotal;
                    var roundingDiff = Math.Round(roundingDiffRaw, 2);

                    string hsnCode = hsnGroup.Key;

                    ws.Cell(excelRow, 1).Value = bill.BillDate.ToString("dd-MM-yyyy");
                    ws.Cell(excelRow, 2).Value = hsnCode; // HSN code from item
                    ws.Cell(excelRow, 3).Value = billID;
                    ws.Cell(excelRow, 4).Value = customer.Name;
                    ws.Cell(excelRow, 5).Value = bill.VehicleNo;
                    ws.Cell(excelRow, 6).Value = bill.Vehicle.ModelName;
                    ws.Cell(excelRow, 7).Value = customer.GSTIN;
                    ws.Cell(excelRow, 8).Value = 28;
                    ws.Cell(excelRow, 9).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 9).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 10).Value = totalCGST;
                    ws.Cell(excelRow, 10).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 11).Value = totalSGST;
                    ws.Cell(excelRow, 11).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 12).Value = roundingDiff;
                    ws.Cell(excelRow, 13).Value = total;
                    excelRow++;
                }
            }
            using var ms = new MemoryStream();
            workBook.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] GenerateExcelPurchase(PurchaseDetails[] purchaseBills, string details)
        {
            using var workBook = new XLWorkbook();
            var ws = workBook.Worksheets.Add(details);

            ws.Cell(1, 1).Value = "DATE";
            
            ws.Cell(1, 2).Value = "Invoice Number";
            ws.Cell(1, 3).Value = "Supplier NAME";
            
            ws.Cell(1, 4).Value = "GST NO";
            ws.Cell(1, 5).Value = "TAX %";
            ws.Cell(1, 6).Value = "TAX VALUE";
            ws.Cell(1, 7).Value = "CGST";
            ws.Cell(1, 8).Value = "SGST";
            ws.Cell(1, 9).Value = "ROUND";
            ws.Cell(1, 10).Value = "TOTAL";
            var headerRow = ws.Row(1);
            headerRow.Style.Font.FontColor = XLColor.Red;
            int excelRow = 2; // Start writing from Excel row 2

            for (int row = 0; row < purchaseBills.Length; row++)
            {
                var purchase = purchaseBills[row].purchase;
                var customer = purchaseBills[row].purchase.Supplier;
                

                // --- SERVICE ITEMS (Tax 18%) ---
                var purchaseItems = purchaseBills[row].purchaseItems;
                    

                if (purchaseItems.Any())
                {
                    var totalTaxableAmount = 0m;
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalAmount = 0m;
                    foreach(var item in purchaseItems)
                    {
                        var subTotal = item.Quantity * item.UnitPrice;
                        decimal discount = 0;

                        if (item.DiscountType == DiscountType.AMOUNT)
                        {
                            discount = item.DiscountScope == DiscountScope.PER_UNIT
                            ? item.DiscountValue * item.Quantity
                            : item.DiscountValue;
                        }
                        else if (item.DiscountType == DiscountType.PERCENT)
                        {
                            discount = item.DiscountScope == DiscountScope.PER_UNIT
                            ? (item.UnitPrice * item.DiscountValue / 100m) * item.Quantity
                            : (subTotal * item.DiscountValue / 100m);
                        }


                        purchase.DiscountAmount += discount;
                        var total = (subTotal - discount) + item.FrightValue;
                        var cgst = 0m;
                        var sgst = 0m;
                        if (item != null)
                        {
                            item.TaxableValue = purchase.taxType == TaxType.INCLUSIVE_TAX ? (item.TaxRate == TaxRate.TAX_18 ? Math.Round(total / 1.18m, 2) : Math.Round(total / 1.28m, 2)) : total;
                            cgst = item.TaxRate == TaxRate.TAX_18 ? Math.Round(item.TaxableValue * (9 / 100m), 2) : Math.Round(item.TaxableValue * (14 / 100m), 2);
                            sgst = item.TaxRate == TaxRate.TAX_18 ? Math.Round(item.TaxableValue * (9 / 100m), 2) : Math.Round(item.TaxableValue * (14 / 100m), 2);
                            item.TotalPrice = Math.Round(item.TaxableValue + cgst + sgst);

                            totalTaxableAmount += item.TaxableValue;
                            totalCGST += cgst;
                            totalSGST += sgst;
                            totalAmount += item.TotalPrice;
                        }
                    }

                    var computedTotal = totalTaxableAmount + totalCGST + totalSGST;

                    // ✅ Rounding difference
                    var roundingDiffRaw = totalAmount - computedTotal;
                    var roundingDiff = Math.Round(roundingDiffRaw, 2);

                    ws.Cell(excelRow, 1).Value = purchase.PurchaseDate.ToString("dd-MM-yyyy");
                    
                    ws.Cell(excelRow, 2).Value = purchase.InvoiceNumber;
                    ws.Cell(excelRow, 3).Value = customer.Name;
                    
                    ws.Cell(excelRow, 4).Value = customer.GSTIN;
                    ws.Cell(excelRow, 5).Value = 28;
                    ws.Cell(excelRow, 6).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 6).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 7).Value = totalCGST;
                    ws.Cell(excelRow, 7).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 8).Value = totalSGST;
                    ws.Cell(excelRow, 8).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 9).Value = roundingDiff;
                    ws.Cell(excelRow, 10).Value = totalAmount;
                    excelRow++;
                }

                
            }
            using var ms = new MemoryStream();
            workBook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
