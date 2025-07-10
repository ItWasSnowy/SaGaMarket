// Controllers/MediaController.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SaGaMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MediaController> _logger;
        private const string ImageFolderName = "uploads"; // Константа с именем папки
        private readonly string _imageFolderPath;
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

        public MediaController(IWebHostEnvironment environment, ILogger<MediaController> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Если WebRootPath null, используем ContentRootPath
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            _imageFolderPath = Path.Combine(rootPath, "uploads");

            // Создаем папку для загрузок, если ее нет
            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }
        }

        [HttpPost("upload-image/{variantId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadImage([FromRoute] string variantId, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image file received.");

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                return BadRequest("Unsupported image format.");

            try
            {
                var fileName = $"{variantId}{ext}";
                var filePath = Path.Combine(_imageFolderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                var imageUrl = $"{Request.Scheme}://{Request.Host}/api/media/image/{fileName}";

                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading image");
            }
        }

        [HttpGet("image/{imageName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetImage(string imageName)
        {
            try
            {
                var filePath = Path.Combine(_imageFolderPath, imageName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting image");
            }
        }

        [HttpDelete("delete-image/{variantId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteImage(string variantId)
        {
            try
            {
                var files = Directory.GetFiles(_imageFolderPath, $"{variantId}.*")
                    .Where(f => _allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToList();

                if (files.Count == 0)
                    return NotFound("Image not found.");

                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }

                return Ok("Image deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting file: {ex.Message}");
            }
        }

        [HttpHead("image/{imageName}")]
        public IActionResult HeadImage(string imageName)
        {
            try
            {
                var filePath = Path.Combine(_imageFolderPath, imageName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };

                Response.ContentLength = new FileInfo(filePath).Length;
                Response.ContentType = contentType;

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HEAD request for image");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}