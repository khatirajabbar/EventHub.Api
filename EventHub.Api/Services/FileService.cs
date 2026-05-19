namespace EventHub.Api.Services;

public class FileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}