using ITHelp.Models;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace ITHelp.Services
{
    public interface IFileIOService
    {
        bool CheckDeniedExtension(string extension);
        Task SaveWorkOrderFile(WorkOrders workOrder, IFormFile file);
        FileStream GetWorkOrderFile(WorkOrders workOrder, string fileName);
    }
    public class FileIOService : IFileIOService
    {
        private readonly IConfiguration _configuration;

        public FileIOService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool CheckDeniedExtension(string extension)
        {
            var permittedExtensions = _configuration.GetSection("AllowedFileExtensions").Get<string[]>();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                return true;
            }
            return false;
        }

        public async Task SaveWorkOrderFile(WorkOrders workOrder, IFormFile file)
        {
            var localFolder = $"{GetRoot()}/{workOrder.RequestDate.Value.Year}/";
            await SaveFile(localFolder, file);
        }

        public FileStream GetWorkOrderFile(WorkOrders workOrder, string link)
        {
            var localFolder = $"{GetRoot()}/{workOrder.RequestDate.Value.Year}/";
            var filePath = Path.Combine(localFolder, link);
            return GetFile(filePath);
        }

        private async Task SaveFile(string localFolder, IFormFile file)
        {
            System.IO.Directory.CreateDirectory(localFolder);

            var filePath = Path.Combine(localFolder, file.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
        }

        private string GetRoot()
        {
            return _configuration["FilePath"];
        }

        private FileStream GetFile(string filePath)
        {
            var content = new System.IO.FileStream(filePath, FileMode.Open, FileAccess.Read);
            return content;
        }

    }
}
