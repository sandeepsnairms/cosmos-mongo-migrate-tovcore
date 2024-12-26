﻿using System.Net.Http;
using System.Threading.Tasks;


namespace MongoMigrationWebApp.Service
{
    public class FileService
    {
        private readonly HttpClient _httpClient;

        public FileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

       string GetFileDownloadUrl(string fileName)
        {
            return $"/api/File/download/{fileName}";
        }
    }
}



