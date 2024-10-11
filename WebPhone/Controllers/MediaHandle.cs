using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;

namespace WebPhone.Controllers
{
    public class MediaHandle : Controller
    {
        private readonly ILogger<MediaHandle> _logger;
        private readonly Cloudinary _cloudinary;

        public MediaHandle
            (
                ILogger<MediaHandle> logger,
                Cloudinary cloudinary
            )
        {
            _logger = logger;
            _cloudinary = cloudinary;
        }

        [NonAction]
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return string.Empty;

                // Tạo tên file là duy nhất
                var fileName = Path.GetFileNameWithoutExtension(file.FileName) + DateTime.UtcNow.Ticks + Path.GetExtension(file.FileName);

                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, stream),
                        PublicId = fileName + "_" + DateTime.UtcNow.Ticks,
                        Folder = "WebPhone"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == HttpStatusCode.OK)
                        return uploadResult.SecureUrl.ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi tải lên hình ảnh
                _logger.LogError(ex.Message);
                return string.Empty;
            }
        }

        public void DeleteImage(string filePath)
        {
            try
            {
                // Kiểm tra nếu filePath có tồn tại thì xóa file
                if (System.IO.File.Exists(filePath))
                {
                    // Xóa file
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
