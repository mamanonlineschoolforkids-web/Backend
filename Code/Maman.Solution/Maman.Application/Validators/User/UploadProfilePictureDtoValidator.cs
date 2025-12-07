//using FluentValidation;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Maman.Application.Validators.User
//{
//	public class UploadProfilePictureDtoValidator : AbstractValidator<UploadProfilePictureDto>
//	{
//		private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
//		private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

//		public UploadProfilePictureDtoValidator()
//		{
//			RuleFor(x => x.File)
//				.NotNull().WithMessage("File is required")
//				.Must(file => file != null && file.Length > 0).WithMessage("File cannot be empty")
//				.Must(file => file == null || file.Length <= MaxFileSize)
//					.WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024}MB")
//				.Must(file => file == null || AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
//					.WithMessage($"Only {string.Join(", ", AllowedExtensions)} files are allowed");
//		}
//	}
//}
