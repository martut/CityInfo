using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FilesController(FileExtensionContentTypeProvider contentTypeProvider)
    {
        _contentTypeProvider = contentTypeProvider 
            ?? throw new System.ArgumentNullException(nameof(contentTypeProvider));
    }
    
    
    [HttpGet("{fileId}")]
    public IActionResult GetFile(string fileId)
    {
        var path = "cw_2021.pdf";

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        if (!_contentTypeProvider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        
        var bytes = System.IO.File.ReadAllBytes(path);
        return File(bytes, contentType, Path.GetFileName(path));
    }
}