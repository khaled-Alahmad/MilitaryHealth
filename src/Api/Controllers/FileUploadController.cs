using Api.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]

public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(IWebHostEnvironment env, ILogger<FileUploadController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// رفع ملف وتخزينه في مجلد Uploads/yyyyMMdd مع اسم فريد
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]

    public async Task<IActionResult> UploadFile([FromForm] UploadFile file)
    {
        if (file == null || file.File.Length == 0)
            return BadRequest(new { succeeded = false, message = "No file uploaded" });

        try
        {
            var uploadRoot = Path.Combine(_env.ContentRootPath, "Files");
            if (!Directory.Exists(uploadRoot))
            {
                Directory.CreateDirectory(uploadRoot);
            }

            var ext = Path.GetExtension(file.File.FileName);
            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadRoot, uniqueName);

            await using var fs = new FileStream(fullPath, FileMode.Create);
            await file.File.CopyToAsync(fs);

            var relativePath = Path.Combine("Files", uniqueName).Replace("\\", "/");
            return Ok(new { succeeded = true, path = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed");
            return StatusCode(500, new { succeeded = false, message = ex.Message });
        }
    }
}
