
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Google.Cloud.Storage.V1;
using RAGSERVERAPI.DTOs;

namespace RAGSERVERAPI.Services;

public interface IGcsService
{
    Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, bool isPublic = false);
    Task<byte[]> DownloadFileAsync(string bucketPath);
    Task<string> DownloadDocumentAsync(string bucketPath, string tempFilePath);
    Task<bool> DeleteFileAsync(string bucketPath);
}

public class GcsService : IGcsService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public GcsService(IConfiguration configuration)
    {
        var credentialsPath = configuration["GCP:KeyFilePath"];
        if (!string.IsNullOrEmpty(credentialsPath))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        }

        _storageClient = StorageClient.Create();
        _bucketName = configuration["GCP:BucketName"] ?? throw new ArgumentNullException("GCP:BucketName");
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, bool isPublic = false)
    {
        var objectName = $"documents/{Guid.NewGuid()}/{fileName}";

        using var stream = new MemoryStream(content);
        var uploadOptions = new UploadObjectOptions
        {
            PredefinedAcl = isPublic ? PredefinedObjectAcl.PublicRead : PredefinedObjectAcl.Private
        };

        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            contentType,
            stream,
            uploadOptions
        );

        return $"gs://{_bucketName}/{objectName}";
    }

    public async Task<byte[]> DownloadFileAsync(string bucketPath)
    {
        var objectName = bucketPath.Replace($"gs://{_bucketName}/", "");

        using var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_bucketName, objectName, stream);

        return stream.ToArray();
    }

    public async Task<string> DownloadDocumentAsync(string bucketPath, string tempFilePath)
    {
        var objectName = bucketPath.Replace($"gs://{_bucketName}/", "");

        // Download from GCS
        using var outputFile = File.OpenWrite(tempFilePath);
        await _storageClient.DownloadObjectAsync(
            _bucketName,
            objectName,
            outputFile
        );

        return tempFilePath;
    }
    public async Task<bool> DeleteFileAsync(string bucketPath)
    {
        try
        {
            var objectName = bucketPath.Replace($"gs://{_bucketName}/", "");
            await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
