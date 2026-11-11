namespace RaceDay.Api.Services;

public interface IBlobService
{
    // Saves the image in the blob container and returns its public link
    Task<string> UploadAsync(Stream content, string folder, string fileName, string contentType);
}
