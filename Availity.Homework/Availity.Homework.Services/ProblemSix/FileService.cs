using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Availity.Homework.Services
{
    public class FileService : IFileService
    {
        public FileService() { }

        public async Task<List<string>> SaveFiles(Dictionary<string, string> namesAndContent)
        {
            var filePaths = new List<string>();
            var tempDirectoryPath = Environment.GetEnvironmentVariable("TEMP");
            foreach(var keyValuePair in namesAndContent)
            {
                var fileName = string.Format("{0}{1}", keyValuePair.Key, ".csv");
                var filePath = Path.Combine(tempDirectoryPath, fileName);
                await File.WriteAllTextAsync(filePath, keyValuePair.Value);
                filePaths.Add(filePath);
            }

            return filePaths;
        }
    }
}
