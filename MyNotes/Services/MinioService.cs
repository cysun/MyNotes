using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;

namespace MyNotes.Services;

public class MinioSettings
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Bucket { get; set; } // Space for DigitalOcean
    public string PathPrefix { get; set; } // E.g. "mynotes/", or "" if no folder is used

    // Attachment Types are files (e.g. zip) whose Content-Disposition header should be
    // set to attachment even for View File operation. These files cannot be displayed
    // directly in browser so browsers will try to save them. Providing a file name ensures
    // that the file is saved with the right name instead of having id as its name.
    public HashSet<string> AttachmentTypes { get; set; }

    // Text Types are files (e.g. java) that should be displayed directly in browser.
    // Browsers may not display them because of their content types, so we'll overwrite
    // their content types with "text/plain".
    public HashSet<string> TextTypes { get; set; }
}

public class MinioService
{
    private MinioClient _client;
    private MinioSettings _settings;

    private ILogger<MinioService> _logger;

    public MinioService(IOptions<MinioSettings> settings, ILogger<MinioService> logger)
    {
        _settings = settings.Value;
        _client = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL()
            .Build();

        _logger = logger;
    }

    public bool IsAttachmentType(string fileName)
    {
        return _settings.AttachmentTypes.Contains(Path.GetExtension(fileName).ToLower());
    }

    public bool IsTextType(string fileName)
    {
        return _settings.TextTypes.Contains(Path.GetExtension(fileName).ToLower());
    }

    public string GetObjectName(Models.File file) => GetObjectName(file.Id, file.Version);

    public string GetObjectName(int fileId, int version) => $"{_settings.PathPrefix}{fileId}-{version}";

    public async Task UploadFileAsync(Models.File file, IFormFile uploadedFile)
    {
        var objectName = GetObjectName(file);
        using var data = uploadedFile.OpenReadStream();
        var args = new PutObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(file.Size)
            .WithContentType(file.ContentType);

        try
        {
            await _client.PutObjectAsync(args);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Failed to upload {object}", objectName);
        }
    }

    public async Task<string> GetDownloadUrlAsync(Models.File file, bool inline = false)
    {
        inline = inline && !IsAttachmentType(file.Name);
        var reqParams = new Dictionary<string, string> {
            { "response-content-type", IsTextType(file.Name)? "text/plain" : file.ContentType },
            {"response-content-disposition", inline ? "inline" : @$"attachment; filename=""{file.Name}"""}
        };

        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject($"{_settings.PathPrefix}{file.Id}-{file.Version}")
            .WithExpiry(10) // Download link valid for 10 seconds
            .WithHeaders(reqParams);

        return await _client.PresignedGetObjectAsync(args);
    }

    public async Task DeleteFileAsync(int fileId, int version)
    {
        var objectName = GetObjectName(fileId, version);
        var args = new RemoveObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(objectName);

        try
        {
            await _client.RemoveObjectAsync(args);
        }
        catch (MinioException e)
        {
            _logger.LogError(e, "Failed to remove {object}", objectName);
        }
    }
}
