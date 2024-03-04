namespace VibeConnect.Post.Module.Utilities;

public static class MediaUploadHelper
{
    public static string GetFileType(string contentType, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.StartsWith("image"))
            {
                return "image";
            }

            if (contentType.StartsWith("video"))
            {
                return "video";
            }
        }

        // If content type is not available, infer from file extension
        var fileExtension = Path.GetExtension(fileName)?.ToLower();

        switch (fileExtension)
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
                return "image";
            case ".gif":
                return "gif";
            case ".mp4":
            case ".mov":
            case ".avi":
                return "video";
            default:
                // Default to "other" if file type cannot be determined
                return "other";
        }
    }

}