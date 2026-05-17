using Microsoft.AspNetCore.Http;

namespace YourPrime.Interfaces;

public interface IBlobService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string fileUrl);
}