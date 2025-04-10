using AutoDynamics.Shared.Services;
using Microsoft.Maui.Controls;

namespace AutoDynamics.Services
{
    class FileHelper : IFileHelper
    {
        public string GetAppPathData(string fileName)
        {
            return Path.Combine(FileSystem.CacheDirectory,fileName);
        }
    }
}
