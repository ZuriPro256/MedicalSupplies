namespace MedicalSupplies.Web.Services;

/// <summary>
/// Minimal local-disk image storage for product photos. Swap the body of
/// SaveAsync for a call to blob/S3-style storage later without touching
/// any controller — they only depend on this interface.
/// </summary>
public interface IFileUploadService
{
    Task<string> SaveProductImageAsync(IFormFile file);
    void DeleteProductImage(string relativeUrl);
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private const string ProductImagesFolder = "images/products";

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveProductImageAsync(IFormFile file)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, ProductImagesFolder);
        Directory.CreateDirectory(uploadsPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{ProductImagesFolder}/{fileName}";
    }

    public void DeleteProductImage(string relativeUrl)
    {
        var fileName = Path.GetFileName(relativeUrl);
        var fullPath = Path.Combine(_env.WebRootPath, ProductImagesFolder, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
