namespace EventHub.Api.Services;

public class FileService
{
    private readonly IWebHostEnvironment _env;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public FileService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        // Validate file is not null
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null.");

        // Validate file size
        if (file.Length > _maxFileSize)
            throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB.");

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}