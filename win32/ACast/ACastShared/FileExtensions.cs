using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ACastShared
{
    public static class FileExtensions
    {
        public static async Task<bool> FileExists(this StorageFolder folder, string fileName)
        {
            try { StorageFile file = await folder.GetFileAsync(fileName); }
            catch { return false; }
            return true;
        }

        public static async Task<bool> FileExist2(this StorageFolder folder, string fileName)
        { return (await folder.GetFilesAsync()).Any(x => x.Name.Equals(fileName)); }
    }
}
