using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerLibrary.Extensions
{
    public static class BlobExtensions
    {
        public static async Task SerializeObjectToBlobAsync(this BlobClient blob, object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] outBuff = Encoding.ASCII.GetBytes(json);
            MemoryStream uploadStream = new MemoryStream(outBuff);
            await blob.UploadAsync(uploadStream, true);
        }
    }
}
