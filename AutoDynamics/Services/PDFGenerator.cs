using AutoDynamics.Shared.Services;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using Microsoft.JSInterop;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Modals.Billing;
namespace AutoDynamics.Services

{
    public class StatementPageEvent : PdfPageEventHelper
    {
        public decimal CarriedForwardBalance = 0;
        public iTextSharp.text.Font Font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9);

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            if (writer.PageNumber > 1)
            {
                PdfContentByte cb = writer.DirectContent;
                ColumnText.ShowTextAligned(cb, iTextSharp.text.Element.ALIGN_RIGHT,
                    new Phrase($"Balance c/f: ₹{CarriedForwardBalance:0.00}", Font),
                    document.Right, document.Bottom - 10, 0);
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            if (writer.PageNumber > 1)
            {
                PdfPTable bfTable = new PdfPTable(6) { WidthPercentage = 100 };
                bfTable.SetWidths(new float[] { 15f, 10f, 35f, 10f, 10f, 10f });

                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9);
                AddBroughtForwardRow(bfTable, CarriedForwardBalance, font);

                document.Add(bfTable);
            }
        }

        private void AddBroughtForwardRow(PdfPTable table, decimal balance, iTextSharp.text.Font font)
        {
            table.AddCell(new PdfPCell(new Phrase("", font)) { Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase("Balance b/f", font)) { Border = Rectangle.NO_BORDER, Colspan = 4 });
            table.AddCell(new PdfPCell(new Phrase(balance.ToString("0.00"), font)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
        }
    }

    public class PDFGenerator : IPDFGenerator
    {

        public async Task<string> GeneratePdfAsync(BillDetails billDetails,IJSRuntime js)
        {
            try
            {
                string query = $"SELECT Name,GSTIN,Contact FROM Customers WHERE CustomerID = '{billDetails.Bill.CustomerID}'";
                DatabaseHandler databaseHandler = new DatabaseHandler();
                var res = await databaseHandler.ExecuteQueryAsync(query);
                string GSTIN = res[0]["GSTIN"].ToString() ?? "";
                string Name = res[0]["Name"].ToString() ?? "";
                string contact = res[0]["Contact"].ToString() ?? "";

                string vehicleDetail = "N/A"; // Default value if no vehicle details are found

                if (!string.IsNullOrEmpty(billDetails.Bill.VehicleNo)) // Check if VehicleNo is not null/empty
                {
                    string vehicleQuery = $"SELECT * FROM Vehicle WHERE VehicleNo = '{billDetails.Bill.VehicleNo}'";
                    var data = await databaseHandler.ExecuteQueryAsync(vehicleQuery);

                    if (data.Count > 0) // Ensure data is not empty
                    {
                        string vehicleMake = data[0]["VehicleMake"]?.ToString() ?? "";
                        string vehicleModel = data[0]["ModelName"]?.ToString() ?? "";
                        vehicleDetail = $"{vehicleMake}\n{vehicleModel}".Trim(); // Trim to remove extra newlines
                    }
                }

                List<string> paymentTypes = new List<string>();

                if (billDetails.BillPayment.CashAmount > 0)
                {
                    paymentTypes.Add("Cash");
                }
                if (billDetails.BillPayment.BankAmount > 0)
                {
                    paymentTypes.Add("Credit");
                }
                if (billDetails.BillPayment.CardAmount > 0)
                {
                    paymentTypes.Add("Card");
                }
                if (billDetails.BillPayment.UPIAmount > 0)
                {
                    paymentTypes.Add("UPI");
                }

                string createPaymentTypeString = string.Join(", ", paymentTypes);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    string fontPath = @"C:\Windows\Fonts\tahoma.ttf";
                    string boldFontPath = @"C:\Windows\Fonts\tahomabd.ttf"; // Bold
                    BaseFont tahomaBaseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    BaseFont tahomaBoldBaseFont = BaseFont.CreateFont(boldFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\seguisym.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font symbolFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                    Document document = new Document(PageSize.A4,15f, 15f, 15f, 15f);
                    
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // ✅ Fonts & Colors (Using Tahoma)
                    iTextSharp.text.Font titleFont = new iTextSharp.text.Font(tahomaBoldBaseFont, 22, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                    iTextSharp.text.Font headerFont = new iTextSharp.text.Font(tahomaBoldBaseFont, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(tahomaBaseFont, 10);
                    iTextSharp.text.Font boldFont = new iTextSharp.text.Font(tahomaBoldBaseFont, 10, iTextSharp.text.Font.BOLD);

                    // ✅ Header (Blue Background)
                    PdfPTable headerTable = new PdfPTable(1);
                    headerTable.WidthPercentage = 100;
                    PdfPCell headerCell = new PdfPCell(new Phrase("AUTO DYNAMICS", titleFont))
                    {
                        BackgroundColor = new BaseColor(0, 102, 204), // Blue
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER,
                        Padding = 10,
                        Border = Rectangle.NO_BORDER
                    };
                    headerTable.AddCell(headerCell);
                    document.Add(headerTable);

                    string address = billDetails.Bill.Branch == "Sivakasi" ?"232/1, TTL Road, Near SFR College, Sivakasi" : "33-I, Kamak Road, Sivakasi" ;

                    document.Add(new Paragraph(address, normalFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
                    document.Add(new Paragraph("GSTIN: 33AJXPA5555E2Z5", normalFont) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
                    document.Add(new Paragraph("\u260E 99444 57589", symbolFont) { Alignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    document.Add(new Paragraph("\u260E 97515 32515", symbolFont) { Alignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    document.Add(new Paragraph("\n"));

                    // ✅ Bill Information (Left + Right Date)
                    PdfPTable billInfoTable = new PdfPTable(2);
                    billInfoTable.WidthPercentage = 100;
                    billInfoTable.SetWidths(new float[] { 1, 1 });

                    string BillId = billDetails.Bill.Branch == "Sivakasi" ? "SFR"+billDetails.Bill.BillNo.ToString().PadLeft(4,'0') : "BPR" + billDetails.Bill.BillNo.ToString().PadLeft(4, '0');
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Bill No: {BillId}", boldFont)) { Border = Rectangle.NO_BORDER });
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Date: {billDetails.Bill.BillDate:dd-MM-yyyy}", boldFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
                
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Customer: {billDetails.Bill.CustomerID}", normalFont)) { Border = Rectangle.NO_BORDER });
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Usage Reading: {billDetails.Bill.UsageReading}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    
                    

                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Name: {Name}", normalFont)) { Border = Rectangle.NO_BORDER });
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Vehicle: {billDetails.Bill.VehicleNo ?? "N/A"}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    

                    billInfoTable.AddCell(new PdfPCell(new Phrase($"M: {contact}", normalFont)) { Border = Rectangle.NO_BORDER });
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Vehicle Type: {vehicleDetail}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    

                    billInfoTable.AddCell(new PdfPCell(new Phrase($"GSTIN: {GSTIN}", normalFont)) { Border = Rectangle.NO_BORDER });
                    billInfoTable.AddCell(new PdfPCell(new Phrase($"Payment Type: {createPaymentTypeString}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });


                    document.Add(billInfoTable);
                    document.Add(new Paragraph("\n"));

                    // Create a table with one column for the title
                    PdfPTable titleTable = new PdfPTable(1);
                    titleTable.WidthPercentage = 100;

                    // Create a cell for "Tax Invoice"
                    PdfPCell titleCell = new PdfPCell(new Phrase("TAX INVOICE", boldFont));
                    titleCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    titleCell.BorderWidthTop = 1f;   // Top Border
                    titleCell.BorderWidthBottom = 1f; // Bottom Border
                    titleCell.BorderWidthLeft = 0f;   // No Left Border
                    titleCell.BorderWidthRight = 0f;  // No Right Border
                    titleCell.Padding = 5;

                    // Add the cell to the table
                    titleTable.AddCell(titleCell);

                    titleTable.SpacingAfter = 5;

                    // Add this title table **above** your main PdfPTable
                    document.Add(titleTable);


                    // ✅ Items Table
                    PdfPTable table = new PdfPTable(9); // Now 9 columns for better alignment
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 2.5f, 1.5f, 1, 1, 0.7f, 0.7f, 0.7f, 0.7f, 1 });


                    // ✅ First Header Row (Merging CGST & SGST)
                    PdfPCell particularsCell = new PdfPCell(new Phrase("Particulars", headerFont)) { Rowspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };
                    PdfPCell hsnCodeCell = new PdfPCell(new Phrase("HSNCode", headerFont)) { Rowspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };
                    PdfPCell qtyCell = new PdfPCell(new Phrase("Qty", headerFont)) { Rowspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };
                    PdfPCell taxableCell = new PdfPCell(new Phrase("Taxable (\u20B9)", headerFont)) { Rowspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };

                    // ✅ CGST & SGST Merged Headers
                    PdfPCell cgstHeaderCell = new PdfPCell(new Phrase("CGST", headerFont)) { Colspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };
                    PdfPCell sgstHeaderCell = new PdfPCell(new Phrase("SGST", headerFont)) { Colspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };
                    PdfPCell totalCell = new PdfPCell(new Phrase("Total (\u20B9)", headerFont)) { Rowspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) };

                    // ✅ Adding First Row Headers
                    table.AddCell(particularsCell);
                    table.AddCell(hsnCodeCell);
                    table.AddCell(qtyCell);
                    table.AddCell(taxableCell);
                    table.AddCell(cgstHeaderCell);
                    table.AddCell(sgstHeaderCell);
                    table.AddCell(totalCell);

                    // ✅ Second Header Row (Splitting CGST & SGST into Rate & Amt)
                    table.AddCell(new PdfPCell(new Phrase("Rate", headerFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) });
                    table.AddCell(new PdfPCell(new Phrase("Amt", headerFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) });
                    table.AddCell(new PdfPCell(new Phrase("Rate", headerFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) });
                    table.AddCell(new PdfPCell(new Phrase("Amt", headerFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, BackgroundColor = new BaseColor(224, 224, 224) });

                    var totalCGST = 0m;
                    var totalSGST = 0m;
                    var totalQuantity = 0;
                    var totalTaxableAmount = 0m;

                    // ✅ Table Items (Existing logic preserved)
                    foreach (var item in billDetails.BillItems)
                    {
                        var cgstRate = item.ItemType == "SERVICE" ? 9 : (item.TaxRate == TaxRate.TAX_18 ? 9 : 14);
                        var sgstRate = item.ItemType == "SERVICE" ? 9 : (item.TaxRate == TaxRate.TAX_18 ? 9 : 14);
                        var cgstAmt = Math.Round(item.TaxableValue * (cgstRate / 100m), 2);
                        var sgstAmt = Math.Round(item.TaxableValue * (sgstRate / 100m), 2);

                        totalCGST += cgstAmt;
                        totalSGST += sgstAmt;
                        totalQuantity += item.Quantity;
                        totalTaxableAmount += item.TaxableValue;
                        var hsnCode = item.ItemType == "SERVICE" ? "998729" : item.HSNCode;
                        table.AddCell(new PdfPCell(new Phrase(item.ItemName, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(hsnCode, normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(item.TaxableValue.ToString("F2"), normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });

                        // ✅ CGST Rate & Amount
                        table.AddCell(new PdfPCell(new Phrase($"{cgstRate}%", normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(cgstAmt.ToString("F2"), normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });

                        // ✅ SGST Rate & Amount
                        table.AddCell(new PdfPCell(new Phrase($"{sgstRate}%", normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(sgstAmt.ToString("F2"), normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });

                        table.AddCell(new PdfPCell(new Phrase(Math.Round(item.TotalPrice).ToString("F2"), normalFont)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT });
                    }

                    document.Add(table);
                    document.Add(new Paragraph("\n"));


                    // ✅ Tax & Total Section in Single Row
                    PdfPTable totalTable = new PdfPTable(7); // Use 7 columns to match the main table
                    totalTable.WidthPercentage = 100;
                    totalTable.SetWidths(new float[] { 2, 1, 1, 1, 1, 1, 1 }); // Match column widths with the main table

                    // ✅ Empty cell for "Particulars" to align correctly
                    totalTable.AddCell(new PdfPCell(new Phrase($"Total", boldFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT,
                        Colspan = 2 // Span across "Particulars", "HSNCode", and "Qty"
                    });

                    totalTable.AddCell(new PdfPCell(new Phrase($"{totalQuantity}", normalFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER
                    });

                    // ✅ Add Total Values
                    totalTable.AddCell(new PdfPCell(new Phrase($"₹{totalTaxableAmount:F2}", normalFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT
                    });

                    // ✅ CGST & SGST total should match structure (Rate + Amt)
                    totalTable.AddCell(new PdfPCell(new Phrase($"₹{totalCGST:F2}", normalFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT
                    });
                    totalTable.AddCell(new PdfPCell(new Phrase($"₹{totalSGST:F2}", normalFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT
                    });

                    // ✅ Final total amount
                    totalTable.AddCell(new PdfPCell(new Phrase($"₹{Math.Round(billDetails.Bill.TotalAmount).ToString("F2")}", boldFont))
                    {
                        HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT
                    });

                    totalTable.SpacingBefore = 150;
                    document.Add(totalTable);
                    Paragraph amountParagraph = new Paragraph();
                    amountParagraph.Add(new Chunk("Amount in Words: ", boldFont)); // Bold for label
                    amountParagraph.Add(new Chunk($"{ConvertAmountToWords(Math.Round(billDetails.Bill.GrandTotal))} Only", normalFont)); // Normal text for amount and "Only"
                    document.Add(amountParagraph);


                    document.Add(new Paragraph("\n"));

                    document.Add(new Paragraph($"Customer Signature:", boldFont));
                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph($"___________________________________________", boldFont));
                    document.Add(new Paragraph("\n"));

                    Paragraph paragraph = new Paragraph("Wheel Alignment Free Check up should be done within 2500 kms. Otherwise, Free Check up cannot be done.", boldFont);
                    paragraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER; // Centers the paragraph

                    document.Add(paragraph);

                    // ✅ Bank Details & Signature
                    PdfPTable footerTable = new PdfPTable(2);
                    footerTable.WidthPercentage = 100;
                    footerTable.SetWidths(new float[] { 70, 30 }); // Adjust column width ratio

                    PdfPCell bankDetailsCell = new PdfPCell();
                    bankDetailsCell.Border = PdfPCell.NO_BORDER;
                    bankDetailsCell.AddElement(new Paragraph("BANK DETAILS", boldFont));
                    bankDetailsCell.AddElement(new Paragraph("Account Holder: AUTO DYNAMICS", normalFont));
                    bankDetailsCell.AddElement(new Paragraph("Bank: UNION BANK OF INDIA", normalFont));
                    bankDetailsCell.AddElement(new Paragraph("A/C No: 335501010035528 (IFSC: UBIN0533556)", normalFont));
                    bankDetailsCell.AddElement(new Paragraph("Branch: THIRUTTANGAL", normalFont));
                    footerTable.AddCell(bankDetailsCell);

                    PdfPCell signatureCell = new PdfPCell();
                    signatureCell.Border = PdfPCell.NO_BORDER;
                    signatureCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    signatureCell.AddElement(new Paragraph("For AUTO DYNAMICS", boldFont));
                    signatureCell.AddElement(new Paragraph("_________________", normalFont));
                    signatureCell.AddElement(new Paragraph("Authorized Signatory", normalFont));
                    footerTable.AddCell(signatureCell);
                    footerTable.SpacingBefore = 30;

                    document.Add(footerTable);
                    document.Close();


                    // ✅ Convert MemoryStream to Byte Array
                    byte[] pdfBytes = memoryStream.ToArray();

                    // ✅ Save Temp PDF File
                    string tempFilePath = Path.Combine(FileSystem.CacheDirectory, "TempPrint.pdf");
                    await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

                    // ✅ Open Print Dialog (Windows Only)
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = tempFilePath,  // Open the PDF in default viewer
                            UseShellExecute = true
                        };

                        Process.Start(psi);

                        return "Print dialog opened successfully.";
                    }

                    return "Printing is only implemented for Windows.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error printing PDF: " + ex.Message);
                return "Error printing PDF: " + ex.Message;
            }
        }

        public static string ConvertAmountToWords(decimal amount)
        {
            if (amount == 0) return "Zero";

            string words = "";
            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                       "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            // Split amount into integer and decimal parts
            int intPart = (int)amount;
            int paisePart = (int)((amount - intPart) * 100); // Extract paise (decimal part)

            // Helper function to process numbers below 1000
            string ConvertBelowThousand(int num)
            {
                if (num == 0) return "";
                if (num < 20) return units[num] + " ";
                if (num < 100) return tens[num / 10] + " " + units[num % 10] + " ";
                return units[num / 100] + " Hundred " + ConvertBelowThousand(num % 100);
            }

            // Process the main number
            if (intPart >= 10000000) // Crore
            {
                words += ConvertBelowThousand(intPart / 10000000) + "Crore ";
                intPart %= 10000000;
            }
            if (intPart >= 100000) // Lakh
            {
                words += ConvertBelowThousand(intPart / 100000) + "Lakh ";
                intPart %= 100000;
            }
            if (intPart >= 1000) // Thousand
            {
                words += ConvertBelowThousand(intPart / 1000) + "Thousand ";
                intPart %= 1000;
            }
            if (intPart > 0) // Below 1000
            {
                words += ConvertBelowThousand(intPart);
            }

            // Handle decimal (paise) part
            if (paisePart > 0)
            {
                words += "and " + ConvertBelowThousand(paisePart) + "Paise";
            }

            return words.Trim();
        }


        private void AddOpeningBalance(Document document, decimal openingBalance, iTextSharp.text.Font font)
        {
            PdfPTable openTable = new PdfPTable(6) { WidthPercentage = 100 };
            openTable.SetWidths(new float[] { 15f, 10f, 35f, 10f, 10f, 10f });

            PdfPCell cell = new PdfPCell(new Phrase("Opening Balance", font))
            {
                Colspan = 5,
                Border = Rectangle.BOX,
                Padding = 5f
            };
            openTable.AddCell(cell);
            openTable.AddCell(new PdfPCell(new Phrase(openingBalance.ToString("0.00"), font))
            {
                Border = Rectangle.BOX,
                HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT,
                Padding = 5f
            });

            document.Add(openTable);
        }

        private void AddClosingBalance (Document document, decimal closingBalance, iTextSharp.text.Font font)
        {
            PdfPTable openTable = new PdfPTable(6) { WidthPercentage = 100 };
            openTable.SetWidths(new float[] { 15f, 10f, 35f, 10f, 10f, 10f });

            PdfPCell cell = new PdfPCell(new Phrase("Cloasing Balance", font))
            {
                Colspan = 5,
                Border = Rectangle.BOX,
                Padding = 5f
            };
            openTable.AddCell(cell);
            openTable.AddCell(new PdfPCell(new Phrase(closingBalance.ToString("0.00"), font))
            {
                Border = Rectangle.BOX,
                HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT,
                Padding = 5f
            });

            document.Add(openTable);
        }


        public async Task<string> GenerateCustomerStatement(List<CustomerStatement> customerStatements, decimal openingBalance, UserModal customer)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 40, 40, 40, 40);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                    var pageEvent = new StatementPageEvent();
                    writer.PageEvent = pageEvent;

                    document.Open();

                    // Fonts
                    var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var fontSubHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var fontRow = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    // Header
                    Paragraph companyInfo = new Paragraph("AutoDynamics\nCustomer Statement", fontHeader)
                    {
                        Alignment = iTextSharp.text.Element.ALIGN_CENTER
                    };
                    document.Add(companyInfo);

                    Paragraph customerInfo = new Paragraph($"Customer: {customer.Name}\nCustomer ID: {customer.CustomerId}\nDate: {DateTime.Now:dd-MM-yyyy}", fontSubHeader)
                    {
                        SpacingAfter = 10
                    };
                    document.Add(customerInfo);

                    // Table
                    //PdfPTable table = CreateStatementTable(fontSubHeader
                    PdfPTable bodyTable = CreateStatementTable(fontSubHeader);
                    decimal runningBalance = openingBalance;
                    pageEvent.CarriedForwardBalance = openingBalance;
                    document.Add(bodyTable);

                    AddOpeningBalance(document, openingBalance, fontRow);

                    // New table for body
                    

                    //// Opening Balance
                    //AddCell(table, "", fontRow);
                    //AddCell(table, "Opening Balance", fontRow);
                    //AddCell(table, "", fontRow, iTextSharp.text.Element.ALIGN_RIGHT);
                    //AddCell(table, "", fontRow, iTextSharp.text.Element.ALIGN_RIGHT);
                    //AddCell(table, runningBalance.ToString("0.00"), fontRow, iTextSharp.text.Element.ALIGN_RIGHT);
                    //document.Add(table);

                   

                    var grouped = customerStatements.GroupBy(s => s.EntryID);
                    foreach (var group in grouped)
                    {
                        PdfPTable outerGroupTable = new PdfPTable(1)
                        {
                            WidthPercentage = 100,
                            
                        };

                        PdfPCell containerCell = new PdfPCell { Border = Rectangle.BOX, Padding = 0 };

                        PdfPTable innerTable = new PdfPTable(6) { WidthPercentage = 100 };
                        innerTable.SetWidths(new float[] { 15f, 10f, 35f, 10f, 10f, 10f });

                        bool isFirst = true;
                        foreach (var entry in group)
                        {
                            innerTable.AddCell(CreateCell(isFirst ? entry.date.ToString("dd-MM-yyyy") : "", fontRow));
                            innerTable.AddCell(CreateCell(isFirst ? entry.type : "", fontRow));
                            innerTable.AddCell(CreateCell(entry.particulars, fontRow));
                            innerTable.AddCell(CreateCell(entry.debit > 0 ? entry.debit.ToString("0.00") : "", fontRow, iTextSharp.text.Element.ALIGN_RIGHT));
                            innerTable.AddCell(CreateCell(entry.credit > 0 ? entry.credit.ToString("0.00") : "", fontRow, iTextSharp.text.Element.ALIGN_RIGHT));

                            runningBalance += entry.credit - entry.debit;
                            innerTable.AddCell(CreateCell(runningBalance.ToString("0.00"), fontRow, iTextSharp.text.Element.ALIGN_RIGHT,false));

                            isFirst = false;
                        }

                        containerCell.AddElement(innerTable);
                        outerGroupTable.AddCell(containerCell);
                        document.Add(outerGroupTable);
                    }




                    // Closing Balance
                    AddClosingBalance(document, runningBalance, fontRow);

                    document.Close();
                    writer.Close();

                    byte[] pdfBytes = memoryStream.ToArray();
                    string tempPath = Path.Combine(FileSystem.CacheDirectory, "CustomerStatement.pdf");
                    await File.WriteAllBytesAsync(tempPath, pdfBytes);

                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        Process.Start(new ProcessStartInfo { FileName = tempPath, UseShellExecute = true });
                        return "Print dialog opened successfully.";
                    }

                    return "PDF generated successfully.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private PdfPCell CreateCell(string text, iTextSharp.text.Font font, int align = iTextSharp.text.Element.ALIGN_LEFT,bool isLeft = true)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Border = isLeft ? Rectangle.RIGHT_BORDER : Rectangle.NO_BORDER,
                PaddingTop = 4f,
                PaddingBottom = 4f,
                HorizontalAlignment = align
            };
        }


        // ✅ Helper - Create Statement Table
        private PdfPTable CreateStatementTable(iTextSharp.text.Font headerFont)
        {
            PdfPTable table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 15f, 10f, 35f, 10f, 10f, 10f });

            AddCell(table, "Date", headerFont, iTextSharp.text.Element.ALIGN_LEFT, true);
            AddCell(table, "Type", headerFont, iTextSharp.text.Element.ALIGN_LEFT, true);
            AddCell(table, "Particulars", headerFont, iTextSharp.text.Element.ALIGN_LEFT, true);
            AddCell(table, "Debit", headerFont, iTextSharp.text.Element.ALIGN_RIGHT, true);
            AddCell(table, "Credit", headerFont, iTextSharp.text.Element.ALIGN_RIGHT, true);
            AddCell(table, "Balance", headerFont, iTextSharp.text.Element.ALIGN_RIGHT, true);

            return table;
        }

        private static void AddCell(PdfPTable table, string text, iTextSharp.text.Font font, int align = iTextSharp.text.Element.ALIGN_LEFT, bool isHeader = false)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = align,
                PaddingTop = 4f,
                PaddingBottom = 4f,
                BackgroundColor = isHeader ? new BaseColor(230, 230, 230) : BaseColor.WHITE
            };
            table.AddCell(cell);
        }

        private static void AddBalanceRow(PdfPTable table, string text, iTextSharp.text.Font font, bool addBorder)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                Colspan = 6,
                HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT,
                Border = addBorder ? Rectangle.TOP_BORDER : Rectangle.NO_BORDER,
                PaddingTop = 6f,
                PaddingBottom = 6f
            };
            table.AddCell(cell);
        }



    }

}
