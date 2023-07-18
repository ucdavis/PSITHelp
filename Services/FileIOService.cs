using ITHelp.Models;
using Microsoft.Data.SqlClient;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace ITHelp.Services
{
    public interface IFileIOService
    {
        bool CheckDeniedExtension(string extension);
        Task SaveWorkOrderFile(WorkOrders workOrder, int attachId, IFormFile file);
        FileStream GetWorkOrderFile(WorkOrders workOrder, Files attach);
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

        private string GetFolder(WorkOrders wo)
        {
            return $"{GetRoot()}/{wo.RequestDate.Value.Year}/{wo.RequestDate.Value.Month}/{wo.Id}";}

        public async Task SaveWorkOrderFile(WorkOrders workOrder, int attachId, IFormFile file)
        {
            
            var localFolder = GetFolder(workOrder);
            await SaveFile(localFolder, file, attachId);
        }

        public FileStream GetWorkOrderFile(WorkOrders workOrder, Files attach)
        {
            var localFolder = GetFolder(workOrder);
            var filePath = Path.Combine(localFolder, attach.Id + attach.Extension);
            return GetFile(filePath);
        }

        private async Task SaveFile(string localFolder, IFormFile file, int Id)
        {
            System.IO.Directory.CreateDirectory(localFolder);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var filePath = Path.Combine(localFolder, Id + ext);
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
