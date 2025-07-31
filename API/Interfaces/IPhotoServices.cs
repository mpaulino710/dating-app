using System;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoServices
{
    Task<ImageUploadResult> UploadPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
