using Azure.Storage.Blobs;
using ExplorerLibrary.Extensions;
using Newtonsoft.Json;

namespace ExplorerLibrary.Services
{
    public class AzureBlobStorageAccess : IDataAccess
    {
        private readonly string containerName = "explorerdata";
        private readonly string blobName = "StateInfo.json";

        public async Task<T> ReadData<T>(string sql, string connectionString)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var cloudBlockBlob = blobContainerClient.GetBlobClient($"{blobName}");

            if (cloudBlockBlob != null)
            {
                var response = await cloudBlockBlob.DownloadAsync();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    string json = streamReader.ReadToEnd();
                    JsonSerializer serializer = new JsonSerializer();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }

            throw new FileNotFoundException($"Blob '{blobName}' not found.");
        }

        public async Task SaveData<T>(string connectionString, T data, string sql)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            if (!container.Exists()) container.Create();
            BlobClient cloudBlockBlob = container.GetBlobClient($"{blobName}");
            if (cloudBlockBlob != null) await cloudBlockBlob.SerializeObjectToBlobAsync(data);
        }
    }
}
