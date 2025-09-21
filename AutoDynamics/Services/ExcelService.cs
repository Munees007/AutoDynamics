using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            ws.Cell(1, 8).Value = "Quantity";
            ws.Cell(1, 9).Value = "TAX %";
            ws.Cell(1, 10).Value = "TAX VALUE";
            ws.Cell(1, 11).Value ="CGST";
            ws.Cell(1, 12).Value ="SGST";
            ws.Cell(1, 13).Value ="ROUND";
            ws.Cell(1, 14).Value = "TOTAL";
            ws.Cell(1,15).Value = "STATE";
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
                int serviceTaxRate = 0;
                if (serviceItems.Any())
                {
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalQuantity = 0;
                    var totalTaxableAmount = 0m;
                    var total = 0m;
                    
                    foreach (var item in serviceItems)
                    {
                        int taxRate = Int32.Parse(item.TaxRate.ToString().Split('_')[1]);
                        serviceTaxRate = taxRate;
                        var cgstRate =  taxRate/2;
                        var sgstRate =taxRate/2;
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
                    ws.Cell(excelRow, 8).Value = totalQuantity;
                    ws.Cell(excelRow, 9).Value = serviceTaxRate.ToString() ;
                    ws.Cell(excelRow, 10).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 10).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 11).Value = totalCGST;
                    ws.Cell(excelRow, 11).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 12).Value = totalSGST;
                    ws.Cell(excelRow, 12).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 13).Value = roundingDiff;
                    ws.Cell(excelRow, 14).Value = total;
                    ws.Cell(excelRow, 15).Value = bill.isActive ? "ACTIVE" : "CANCELED";
                    excelRow++;
                }

                // --- PRODUCT ITEMS with TaxRate.TAX_28 ---
                var productItems = bills[row].BillItems
                    .Where(i => i.ItemType == "PRODUCT")
                    .GroupBy(i => i.HSNCode); // Group by HSN if multiple
                int productTaxRate = 0;
                foreach (var hsnGroup in productItems)
                {
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalQuantity = 0;
                    var totalTaxableAmount = 0m;
                    var total = 0m;
                    foreach (var item in hsnGroup)
                    {
                        int taxRate = Int32.Parse(item.TaxRate.ToString().Split('_')[1]);
                        productTaxRate = taxRate;
                        var cgstRate = taxRate/2;
                        var sgstRate = taxRate/2;
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
                    ws.Cell(excelRow, 8).Value = totalQuantity;
                    ws.Cell(excelRow, 9).Value = productTaxRate.ToString();
                    ws.Cell(excelRow, 10).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 10).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 11).Value = totalCGST;
                    ws.Cell(excelRow, 11).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 12).Value = totalSGST;
                    ws.Cell(excelRow, 12).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 13).Value = roundingDiff;
                    ws.Cell(excelRow, 14).Value = total;
                    ws.Cell(excelRow, 15).Value = bill.isActive ? "ACTIVE" : "CANCELED";
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
            ws.Cell(1, 5).Value = "Quantity";
            ws.Cell(1, 6).Value = "TAX %";
            ws.Cell(1, 7).Value = "TAX VALUE";
            ws.Cell(1, 8).Value = "CGST";
            ws.Cell(1, 9).Value = "SGST";
            ws.Cell(1, 10).Value = "ROUND";
            ws.Cell(1, 11).Value = "TOTAL";
            var headerRow = ws.Row(1);
            headerRow.Style.Font.FontColor = XLColor.Red;
            int excelRow = 2; // Start writing from Excel row 2

            for (int row = 0; row < purchaseBills.Length; row++)
            {
                var purchase = purchaseBills[row].purchase;
                var customer = purchaseBills[row].purchase.Supplier;
                

                // --- SERVICE ITEMS (Tax 18%) ---
                var purchaseItems = purchaseBills[row].purchaseItems;
                int purchaseTaxRate = 0;

                if (purchaseItems.Any())
                {
                    var totalTaxableAmount = 0m;
                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalAmount = 0m;
                    var totalQuantity = 0;
                    foreach (var item in purchaseItems)
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
                            int taxRate = Int32.Parse(item.TaxRate.ToString().Split('_')[1]);
                            purchaseTaxRate = taxRate;
                            int halfRate = taxRate / 2;
                            decimal inclusiveTaxRate = 1 + (taxRate / 100);
                            item.TaxableValue = purchase.taxType == TaxType.INCLUSIVE_TAX ? (Math.Round(total / inclusiveTaxRate, 2)) : total;
                            cgst = Math.Round(item.TaxableValue * (halfRate / 100m), 2);
                            sgst = Math.Round(item.TaxableValue * (halfRate / 100m), 2);
                            item.TotalPrice = Math.Round(item.TaxableValue + cgst + sgst);
                            totalQuantity += item.Quantity;
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
                    ws.Cell(excelRow, 5).Value = totalQuantity;
                    ws.Cell(excelRow, 6).Value = purchaseTaxRate.ToString();
                    ws.Cell(excelRow, 7).Value = totalTaxableAmount;
                    ws.Cell(excelRow, 7).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 8).Value = totalCGST;
                    ws.Cell(excelRow, 8).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 9).Value = totalSGST;
                    ws.Cell(excelRow, 9).Style.NumberFormat.Format = "0.00";
                    ws.Cell(excelRow, 10).Value = roundingDiff;
                    ws.Cell(excelRow, 11).Value = totalAmount;
                    excelRow++;
                }

                
            }
            using var ms = new MemoryStream();
            workBook.SaveAs(ms);
            return ms.ToArray();
        }
        public async Task<List<List<string>>> readData(IBrowserFile file,int workSheetPosition,int startRow)
        {
            var result = new List<List<string>>();

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);
            var worksheet = workbook.Worksheet(workSheetPosition);
            

            foreach (var row in worksheet.RowsUsed().Where(r => r.RowNumber() >= startRow))
            {
                var rowData = new List<string>();
                foreach (var cell in row.CellsUsed())
                {
                    rowData.Add(cell.GetValue<string>());
                }
                result.Add(rowData);
            }

            return result;
        }


public async Task<string> CreateCreditExcel(string branch, List<CreditRecord> sivakasiCredit = null, List<CreditRecord> bypassCredit = null)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Credit List");

            int row = 1;

            // ✅ Date Row
            worksheet.Cell(row, 1).Value = $"Date: {DateTime.Now:dd/MM/yyyy}";
            worksheet.Range(row, 1, row, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;

            // ✅ AUTO DYNAMICS Header
            worksheet.Cell(row, 1).Value = "AUTO DYNAMICS";
            worksheet.Range(row, 1, row, 6).Merge();
            worksheet.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 18;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // ✅ CREDIT LIST Title
            worksheet.Cell(row, 1).Value = "CREDIT LIST";
            worksheet.Range(row, 1, row, 6).Merge();
            worksheet.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 14;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // ✅ Add branch data
            void AddBranchData(string branchName, List<CreditRecord> credits)
            {
                // 🔹 Branch Title
                worksheet.Cell(row, 1).Value = $"Branch: {branchName}";
                worksheet.Range(row, 1, row, 6).Merge();
                worksheet.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 12;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row++;

                    // 🔹 Header
                    string[] headers = { "Name", "Mobile", "Credit Amount", "Paid Amount", "Remaining Amount" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = headers[i];
                        worksheet.Cell(row, i + 1).Style.Font.SetBold();
                        worksheet.Cell(row, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;

                    // 🔹 Data
                    foreach (var record in credits)
                    {
                        var values = record.GenerateForExcel(); // [Name, Mobile, Credit, Paid, Remaining]

                        for (int j = 0; j < values.Length; j++)
                        {
                            var cell = worksheet.Cell(row, j + 1);

                            // If it's one of the 3 amount columns (index 2,3,4 => j=2,3,4), format as number
                            if (j >= 2)
                            {
                                if (decimal.TryParse(values[j], out decimal amount))
                                {
                                    cell.Value = amount;
                                    cell.Style.NumberFormat.Format = "#,##0.00";
                                }
                                else
                                {
                                    cell.Value = values[j]; // fallback
                                }
                            }
                            else
                            {
                                cell.Value = values[j];
                            }

                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }

                        row++;
                    }
                    row += 2;
                }

                if (branch == "Both")
            {
                AddBranchData("Sivakasi", sivakasiCredit ?? new List<CreditRecord>());
                AddBranchData("Bypass", bypassCredit ?? new List<CreditRecord>());
            }
            else if (branch == "Sivakasi")
            {
                AddBranchData("Sivakasi", sivakasiCredit ?? new List<CreditRecord>());
            }
            else
            {
                AddBranchData("Bypass", bypassCredit ?? new List<CreditRecord>());
            }

            worksheet.Columns().AdjustToContents();

            // ✅ Save Excel file
            string filePath = Path.Combine(FileSystem.CacheDirectory, "CreditList.xlsx");
            workbook.SaveAs(filePath);

            // ✅ Open the file (Windows only)
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);

                return "Excel opened successfully.";
            }

            return "Excel created successfully.";
        }
        catch (Exception ex)
        {
            return "Error generating Excel: " + ex.Message;
        }
    }
        public async Task<string> CreateStockExcel(
    string branch,

    List<StockType> sivakasiCredit = null,
    List<StockType> bypassCredit = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var allCredits = new List<(string Branch, StockType Item)>();

                if (branch == "Both")
                {
                    allCredits.AddRange((sivakasiCredit ?? new()).Select(x => ("Sivakasi", x)));
                    allCredits.AddRange((bypassCredit ?? new()).Select(x => ("Bypass", x)));
                }
                else if (branch == "Sivakasi")
                {
                    allCredits.AddRange((sivakasiCredit ?? new()).Select(x => ("Sivakasi", x)));
                }
                else
                {
                    allCredits.AddRange((bypassCredit ?? new()).Select(x => ("Bypass", x)));
                }

                // ===== New logic: Category-wise worksheets first =====
                var categories = new List<(string Title, Func<(string Branch, StockType Item), bool> Filter)>
{
    ("Two_Wheeler", x => x.Item.Product.tyreFor == TyreFor.TWO_WHEELER),
    ("Four_Wheeler", x => x.Item.Product.tyreFor == TyreFor.FOUR_WHEELER),
    ("Tubes", x => x.Item.Product.tyreFor == TyreFor.TUBE)
};

                foreach (var (title, filter) in categories)
                {
                    var worksheet = workbook.Worksheets.Add(title + "_Category");
                    int row = 1;
                    worksheet.Cell(row, 1).Value = title.Replace("_", " ").ToUpper();
                    worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold().Font.FontSize = 14;
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row += 2;

                    // Group by brand inside category sheet
                    var categoryItems = allCredits.Where(filter)
                                                  .GroupBy(x => x.Item.Product.Brand);

                    foreach (var brandGroup in categoryItems)
                    {
                        string brandName = brandGroup.Key;
                        worksheet.Cell(row, 1).Value = $"Brand: {brandName}";
                        worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold().Font.FontSize = 12;
                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        row++;

                        var branchGroups = brandGroup.GroupBy(x => x.Branch);

                        foreach (var branchGroup in branchGroups)
                        {
                            worksheet.Cell(row, 1).Value = $"Branch: {branchGroup.Key}";
                            worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold();
                            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            row++;

                            string[] headers = { "Product ID", "Brand", "Size", "Pattern", "Tube/Tubeless", "Available Qty" };
                            for (int i = 0; i < headers.Length; i++)
                            {
                                var cell = worksheet.Cell(row, i + 1);
                                cell.Value = headers[i];
                                cell.Style.Font.SetBold();
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            row++;

                            foreach (var (branchName, item) in branchGroup)
                            {
                                var values = item.GenerateForExcel();
                                if (values.Length < 6 || !decimal.TryParse(values[5], out var qty) || qty == 0)
                                    continue;

                                for (int i = 0; i < values.Length; i++)
                                {
                                    var cell = worksheet.Cell(row, i + 1);
                                    if (i >= 2 && decimal.TryParse(values[i], out var number))
                                    {
                                        cell.Value = number;
                                        cell.Style.NumberFormat.Format = "#,##0.00";
                                    }
                                    else
                                    {
                                        cell.Value = values[i];
                                    }
                                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                }
                                row++;
                            }

                            row++; // gap after branch
                        }

                        row += 2; // gap after brand
                    }
                }


                // Group by Brand
                var groupedByBrand = allCredits
                    .Where(x => !string.IsNullOrEmpty(x.Item.Product.Brand))
                    .GroupBy(x => x.Item.Product.Brand);

                foreach (var brandGroup in groupedByBrand)
                {
                    string brandName = brandGroup.Key;
                    var worksheet = workbook.Worksheets.Add(brandName.Length > 31 ? brandName.Substring(0, 31) : brandName);

                    int row = 1;

                    worksheet.Cell(row, 1).Value = $"Brand: {brandName}";
                    worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold().Font.FontSize = 14;
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row += 2;

                    // 🔹 Write category-wise section
                    void WriteCategory(string title, Func<StockType, bool> hsnMatch)
                    {
                        var categoryGroup = brandGroup
                            .Where(x => hsnMatch(x.Item))
                            .GroupBy(x => x.Branch);

                        if (!categoryGroup.Any()) return;

                        worksheet.Cell(row, 1).Value = title.ToUpper();
                        worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold().Font.FontSize = 12;
                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        row++;

                        foreach (var branchGroup in categoryGroup)
                        {
                            worksheet.Cell(row, 1).Value = $"Branch: {branchGroup.Key}";
                            worksheet.Range(row, 1, row, 6).Merge().Style.Font.SetBold();
                            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            row++;

                            string[] headers = { "Product ID", "Brand", "Size", "Pattern", "Tube/Tubeless", "Available Qty" };
                            for (int i = 0; i < headers.Length; i++)
                            {
                                var cell = worksheet.Cell(row, i + 1);
                                cell.Value = headers[i];
                                cell.Style.Font.SetBold();
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                            row++;

                            foreach (var (branchName, item) in branchGroup)
                            {
                                var values = item.GenerateForExcel();

                                // Skip if Available Qty (index 5) is zero or not valid
                                if (values.Length < 6 || !decimal.TryParse(values[5], out var qty) || qty == 0)
                                    continue;

                                for (int i = 0; i < values.Length; i++)
                                {
                                    var cell = worksheet.Cell(row, i + 1);

                                    if (i >= 2 && decimal.TryParse(values[i], out var number))
                                    {
                                        cell.Value = number;
                                        cell.Style.NumberFormat.Format = "#,##0.00";
                                    }
                                    else
                                    {
                                        cell.Value = values[i];
                                    }

                                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                }

                                row++;
                            }


                            row++; // gap after branch
                        }

                        row += 2; // gap after category
                    }

                    // ✅ Call for each category
                    WriteCategory("Two Wheeler", stock => stock.Product.tyreFor == TyreFor.TWO_WHEELER);
                    WriteCategory("Four Wheeler", stock => stock.Product.tyreFor == TyreFor.FOUR_WHEELER);
                    WriteCategory("Tubes", stock => stock.Product.tyreFor == TyreFor.TUBE);
                }

                // Final steps
                string filePath = Path.Combine(FileSystem.CacheDirectory, "StockData.xlsx");
                workbook.SaveAs(filePath);

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    return "Excel opened successfully.";
                }

                return "Excel created successfully.";
            }
            catch (Exception ex)
            {
                return "Error generating Excel: " + ex.Message;
            }
        }


    public async Task<string> GenerateCustomerStatementExcel(List<CustomerStatement> customerStatements, decimal openingBalance, UserModal customer, string StartDate, string EndDate)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Customer Statement");

                // Title
                worksheet.Cell(1, 1).Value = "AutoDynamics - Customer Statement";
                worksheet.Range(1, 1, 1, 6).Merge().Style
                    .Font.SetBold().Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Info
                worksheet.Cell(2, 1).Value = $"Customer: {customer.Name}";
                worksheet.Cell(3, 1).Value = $"Customer ID: {customer.CustomerId}";
                worksheet.Cell(4, 1).Value = $"Date: from {StartDate} to {EndDate}";
                worksheet.Cell(5, 1).Value = $"Opening Balance: {openingBalance:0.00}";

                // Table Header
                var headerRow = 7;
                worksheet.Cell(headerRow, 1).Value = "Date";
                worksheet.Cell(headerRow, 2).Value = "Type";
                worksheet.Cell(headerRow, 3).Value = "Particulars";
                worksheet.Cell(headerRow, 4).Value = "Debit";
                worksheet.Cell(headerRow, 5).Value = "Credit";
                worksheet.Cell(headerRow, 6).Value = "Balance";

                worksheet.Range(headerRow, 1, headerRow, 6).Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Data Rows
                decimal runningBalance = openingBalance;
                var row = headerRow + 1;

                var sortedStatements = customerStatements.OrderBy(s => s.date).ToList();
                foreach (var entry in sortedStatements)
                {
                    worksheet.Cell(row, 1).Value = entry.date.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 2).Value = entry.type;
                    worksheet.Cell(row, 3).Value = entry.particulars;
                    worksheet.Cell(row, 4).Value = entry.debit > 0 ? entry.debit.ToString("0.00") : "";
                    worksheet.Cell(row, 5).Value = entry.credit > 0 ? entry.credit.ToString("0.00") : "";

                    // Balance logic same as PDF
                    if (entry.accountType == "SALES A/C")
                    {
                        runningBalance += entry.credit;
                    }
                    else if (entry.accountType == "CASH A/C" || entry.accountType == "BANK A/C" || entry.accountType == "CARD A/C")
                    {
                        runningBalance -= entry.debit;
                    }
                    else if (entry.accountType == "RECEIPT A/C")
                    {
                        runningBalance -= entry.debit;
                    }

                    worksheet.Cell(row, 6).Value = runningBalance.ToString("0.00");
                    row++;
                }

                // Closing Balance
                worksheet.Cell(row, 5).Value = "Closing Balance:";
                worksheet.Cell(row, 6).Value = runningBalance.ToString("0.00");
                worksheet.Cell(row, 5).Style.Font.SetBold();
                worksheet.Cell(row, 6).Style.Font.SetBold();

                worksheet.Columns().AdjustToContents();

                // Save File
                string tempPath = Path.Combine(FileSystem.CacheDirectory, "CustomerStatement.xlsx");
                workbook.SaveAs(tempPath);

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    Process.Start(new ProcessStartInfo { FileName = tempPath, UseShellExecute = true });
                    return "Excel opened successfully.";
                }

                return "Excel generated successfully.";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }


}


}
