using AutoDynamics.Shared.Modals;
using AutoDynamics.Shared.Services;
using System.Diagnostics;


namespace AutoDynamics.Services
{
    class DownloadExcel : IDownloadExcel
    {
        public string DownloadExcelToDevice(BillDetails[] bills, string details,string branch)
        {
            ExcelService excelService = new ExcelService();
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Build the folder path: Documents/Reports/2025
            string reportsPath = Path.Combine(basePath, "Reports", DateTime.Today.Year.ToString());

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(reportsPath);

            string br = branch == "Sivakasi" ? "SFR" : "BPR";
            // Define the file path
            string filePath = Path.Combine(reportsPath, $"{details}_Report_{br}.xlsx");
            File.WriteAllBytes(filePath, excelService.GenerateExcel(bills,details));

            if(DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                };
                Process.Start(psi);
            }
            return filePath;
        }
    }
}
