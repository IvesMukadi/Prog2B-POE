namespace RaceDay.Api.Helpers;

public static class ImageCheck
{
    // 2 MB limit
    private const long MaxSize = 2 * 1024 * 1024;

    // Returns an error message, or null when the file is fine
    public static string? GetError(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return "Please choose an image file.";
        }
        if (file.Length > MaxSize)
        {
            return "The image must be 2 MB or smaller.";
        }

        // Only jpg and png are allowed
        var extension = Path.GetExtension(file.FileName).ToLower();
        var goodType = file.ContentType == "image/jpeg" || file.ContentType == "image/png";
        var goodExtension = extension == ".jpg" || extension == ".jpeg" || extension == ".png";
        if (!goodType || !goodExtension)
        {
            return "Only jpg and png images are allowed.";
        }

        return null;
    }
}
