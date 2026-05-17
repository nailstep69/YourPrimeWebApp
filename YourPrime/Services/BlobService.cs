using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using YourPrime.Auth;
using YourPrime.Interfaces;

namespace YourPrime.Services;

public class BlobService : IBlobService
{
    private readonly AzureBlobSettings _settings;

    public BlobService(IOptions<AzureBlobSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var blobContainerClient = new BlobContainerClient(
            _settings.ConnectionString,
            _settings.ContainerName);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        var blobContainerClient = new BlobContainerClient(
            _settings.ConnectionString,
            _settings.ContainerName);

        var uri = new Uri(fileUrl);

        var fileName = Path.GetFileName(uri.LocalPath);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }
}