using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medical.API.Attributes;
using System.Security.Claims;

namespace Medical.API.Controllers;

/// <summary>
/// 文件上传控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <param name="file">图片文件</param>
    /// <param name="category">分类（activity=活动图片，user=用户头像，healthknowledge=健康知识图片，doctor=医生头像，product=商品图片，aidiagnosis=AI预诊图片，默认为通用图片）</param>
    /// <returns>图片路径</returns>
    [HttpPost("image")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous] // 允许匿名访问，AI预诊需要
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadImage(IFormFile file, [FromQuery] string? category = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件类型
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "不支持的文件类型，仅支持：jpg, jpeg, png, gif, webp" });
        }

        // 验证文件大小（最大 5MB）
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "文件大小不能超过 5MB" });
        }

        try
        {
            // 根据分类确定上传路径
            string uploadSubFolder;
            string relativePathPrefix;
            
            if (category == "activity")
            {
                uploadSubFolder = Path.Combine("uploads", "images", "activity");
                relativePathPrefix = "/uploads/images/activity";
            }
            else if (category == "user")
            {
                uploadSubFolder = Path.Combine("uploads", "images", "users", "admin");
                relativePathPrefix = "/uploads/images/users/admin";
            }
            else if (category == "healthknowledge")
            {
                uploadSubFolder = Path.Combine("uploads", "images", "healthknowledge");
                relativePathPrefix = "/uploads/images/healthknowledge";
            }
            else if (category == "doctor")
            {
                uploadSubFolder = Path.Combine("uploads", "images", "doctors");
                relativePathPrefix = "/uploads/images/doctors";
            }
            else if (category == "product" || category == "products")
            {
                uploadSubFolder = Path.Combine("uploads", "images", "Products");
                relativePathPrefix = "/uploads/images/Products";
            }
            else if (category == "aidiagnosis")
            {
                // 获取当前登录用户名，如果未登录则使用 "anonymous"
                var username = User?.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username))
                {
                    username = "anonymous";
                }
                
                // 清理用户名中的非法字符，确保可以作为文件夹名称
                var safeUsername = string.Join("_", username.Split(Path.GetInvalidFileNameChars()));
                
                uploadSubFolder = Path.Combine("uploads", "images", "users", safeUsername);
                relativePathPrefix = $"/uploads/images/users/{safeUsername}";
            }
            else
            {
                // 默认路径
                uploadSubFolder = Path.Combine("uploads", "images");
                relativePathPrefix = "/uploads/images";
            }

            // 创建上传目录
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, uploadSubFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对路径
            var relativePath = $"{relativePathPrefix}/{fileName}";
            
            _logger.LogInformation("图片上传成功: {FileName}, 路径: {Path}, 分类: {Category}", fileName, relativePath, category ?? "default");

            return Ok(new { 
                url = relativePath,
                path = relativePath,
                fileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图片上传失败");
            return StatusCode(500, new { message = "图片上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传视频
    /// </summary>
    /// <param name="file">视频文件</param>
    /// <returns>视频路径</returns>
    [HttpPost("video")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件类型
        var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "不支持的文件类型，仅支持：mp4, avi, mov, wmv, flv, webm" });
        }

        // 验证文件大小（最大 50MB）
        const long maxFileSize = 50 * 1024 * 1024; // 50MB
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "文件大小不能超过 50MB" });
        }

        try
        {
            // 创建上传目录
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "videos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对路径
            var relativePath = $"/uploads/videos/{fileName}";
            
            _logger.LogInformation("视频上传成功: {FileName}, 路径: {Path}", fileName, relativePath);

            return Ok(new { 
                url = relativePath,
                path = relativePath,
                fileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "视频上传失败");
            return StatusCode(500, new { message = "视频上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传语音
    /// </summary>
    /// <param name="file">语音文件</param>
    /// <returns>语音路径</returns>
    [HttpPost("audio")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件类型
        var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".wma" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "不支持的文件类型，仅支持：mp3, wav, ogg, m4a, aac, wma" });
        }

        // 验证文件大小（最大 10MB）
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "文件大小不能超过 10MB" });
        }

        try
        {
            // 创建上传目录
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "audios");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对路径
            var relativePath = $"/uploads/audios/{fileName}";
            
            _logger.LogInformation("语音上传成功: {FileName}, 路径: {Path}", fileName, relativePath);

            return Ok(new { 
                url = relativePath,
                path = relativePath,
                fileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "语音上传失败");
            return StatusCode(500, new { message = "语音上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传文件（通用文件上传）
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="category">分类（aidiagnosis=AI预诊文件，默认为通用文件）</param>
    /// <returns>文件路径</returns>
    [HttpPost("file")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous] // 允许匿名访问，AI预诊需要
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadFile(IFormFile file, [FromQuery] string? category = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件大小（最大 20MB）
        const long maxFileSize = 20 * 1024 * 1024; // 20MB
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "文件大小不能超过 20MB" });
        }

        try
        {
            // 根据分类确定上传路径
            string uploadSubFolder;
            string relativePathPrefix;
            
            if (category == "aidiagnosis")
            {
                // 获取当前登录用户名，如果未登录则使用 "anonymous"
                var username = User?.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username))
                {
                    username = "anonymous";
                }
                
                // 清理用户名中的非法字符，确保可以作为文件夹名称
                var safeUsername = string.Join("_", username.Split(Path.GetInvalidFileNameChars()));
                
                uploadSubFolder = Path.Combine("uploads", "files", "users", safeUsername);
                relativePathPrefix = $"/uploads/files/users/{safeUsername}";
            }
            else
            {
                uploadSubFolder = Path.Combine("uploads", "files");
                relativePathPrefix = "/uploads/files";
            }

            // 创建上传目录
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, uploadSubFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 获取文件扩展名
            var fileExtension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = ".bin";
            }

            // 生成唯一文件名（保留原始文件名的一部分）
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = string.IsNullOrEmpty(originalFileName) 
                ? Guid.NewGuid().ToString() 
                : $"{Guid.NewGuid()}_{originalFileName}";
            var fileName = $"{safeFileName}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对路径
            var relativePath = $"{relativePathPrefix}/{fileName}";
            
            _logger.LogInformation("文件上传成功: {FileName}, 路径: {Path}", fileName, relativePath);

            return Ok(new { 
                url = relativePath,
                path = relativePath,
                fileName = fileName,
                originalFileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败");
            return StatusCode(500, new { message = "文件上传失败，请稍后重试" });
        }
    }
}

