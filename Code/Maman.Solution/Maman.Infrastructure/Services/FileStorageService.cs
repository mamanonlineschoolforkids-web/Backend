using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Maman.Application.Interfaces;
using Maman.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maman.Application.Services.Utility;

public class FileStorageService : IFileStorageService
{
	private readonly AzureStorageSettings _settings;
	private readonly BlobServiceClient _blobServiceClient;
	private readonly ILogger<FileStorageService> _logger;

	public FileStorageService(IOptions<AzureStorageSettings> settings, ILogger<FileStorageService> logger)
	{
		_settings = settings.Value;
		_logger = logger;
		_blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
	}

	public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName, CancellationToken cancellationToken = default)
	{
		try
		{
			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

			var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
			var blobClient = containerClient.GetBlobClient(uniqueFileName);

			await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

			_logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
			return blobClient.Uri.ToString();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
			throw;
		}
	}

	public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
	{
		try
		{
			var uri = new Uri(fileUrl);
			var blobClient = new BlobClient(uri);
			await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

			_logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
			return false;
		}
	}

	public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
	{
		try
		{
			var uri = new Uri(fileUrl);
			var blobClient = new BlobClient(uri);
			var response = await blobClient.DownloadAsync(cancellationToken);
			return response.Value.Content;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to download file: {FileUrl}", fileUrl);
			throw;
		}
	}

	public async Task<string> GetFileUrlAsync(string fileName, string containerName)
	{
		var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
		var blobClient = containerClient.GetBlobClient(fileName);
		return await Task.FromResult(blobClient.Uri.ToString());
	}
}

