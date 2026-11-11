using RaceDay.Api.Services;

namespace RaceDay.Api.Tests;

// Pretends to upload so the tests do not need Azure
public class FakeBlobService : IBlobService
{
    public Task<string> UploadAsync(Stream content, string folder, string fileName, string contentType)
    {
        return Task.FromResult($"https://fake.blob/{folder}/{fileName}");
    }
}
