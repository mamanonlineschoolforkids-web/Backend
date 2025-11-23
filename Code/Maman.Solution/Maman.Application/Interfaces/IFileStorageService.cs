using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Interfaces
{
	public interface IFileStorageService
	{
		Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName, CancellationToken cancellationToken = default);
		Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
		Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
		Task<string> GetFileUrlAsync(string fileName, string containerName);
	}
}
