using AutoDynamics.Shared.Services;
namespace AutoDynamics.Web.Services
{
    public class FileHelper : IFileHelper
    {
        public string GetAppPathData(string fileName)
        {
            return "/images/" + fileName;
        }
    }
}
