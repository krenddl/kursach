using AuthApi.CustomAtributes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController
    {
        private readonly IWebHostEnvironment _environment;

        public ImageController(IWebHostEnvironment environment)
        { _environment = environment; }

        [HttpPost("MovieImage")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> UploadMovieImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult(new
                    {
                        status = false,
                        message = "Файл не выбран"
                    });
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "movies");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/images/movies/{fileName}";

                return new OkObjectResult(new
                {
                    status = true,
                    img = relativePath
                });
            }
            catch(Exception ex)
            {
                return new ObjectResult(new
                {
                    status = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                })
                {
                    StatusCode = 500
                };
            }
                
            
            
        }
    }
}
