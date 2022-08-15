using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Availity.Homework.Services
{
    public interface IFileService
    {
        Task<List<string>> SaveFiles(Dictionary<string, string> namesAndContent);
    }
}
