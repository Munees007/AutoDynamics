using AutoDynamics.Shared.Modals;
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
            for (int row=0;row<bills.Length;row++)
            {
                var taxableValue = Math.Round(bills[row].Bill.TotalAmount / 1.18m, 2);
                var cgst = Math.Round(taxableValue * (9 / 100m), 2);
                var sgst = Math.Round(taxableValue * (9 / 100m), 2);
                var roundingDiffRaw = bills[row].Bill.TotalAmount - (taxableValue + cgst + sgst);
                var roundingDiff = roundingDiffRaw > 0 ? Math.Round(roundingDiffRaw, 2) : 0m;
                string billID = bills[row].Bill.Branch == "Sivakasi" ? "SFR" + bills[row].Bill.BillNo.ToString().PadLeft(4, '0') : "BPR" + bills[row].Bill.BillNo.ToString().PadLeft(4, '0');
                ws.Cell(row + 2, 1).Value = bills[row].Bill.BillDate.ToString("dd-MM-yyyy");
                ws.Cell(row + 2, 2).Value = 998729;
                ws.Cell(row + 2, 3).Value = billID;
                ws.Cell(row + 2, 4).Value = bills[row].customer.Name;
                ws.Cell(row + 2, 5).Value = bills[row].Bill.VehicleNo;
                ws.Cell(row + 2, 6).Value = bills[row].Bill.Vehicle.ModelName;
                ws.Cell(row + 2, 7).Value = bills[row].customer.GSTIN;
                ws.Cell(row + 2, 8).Value = 18;
                ws.Cell(row + 2, 9).Value = taxableValue;
                ws.Cell(row + 2, 10).Value = cgst;
                ws.Cell(row + 2, 11).Value = sgst;
                ws.Cell(row + 2, 12).Value = roundingDiff;
                ws.Cell(row + 2, 13).Value = bills[row].Bill.TotalAmount;
            }
            using var ms = new MemoryStream();
            workBook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
