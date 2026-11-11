using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public class BlobService : IBlobService
{
    private readonly IConfiguration _config;

    public BlobService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> UploadAsync(Stream content, string folder, string fileName, string contentType)
    {
        // The connection string comes from user secrets or an environment variable
        var connectionString = _config["Blob:ConnectionString"];
        var containerName = _config["Blob:Container"] ?? "raceday-images";
        var container = new BlobContainerClient(connectionString, containerName);

        // Make the container the first time, with public read so the browser can show the images
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blob = container.GetBlobClient($"{folder}/{fileName}");
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });

        return blob.Uri.ToString();
    }
}
