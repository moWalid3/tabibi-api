using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tabibi.API.DTOs.Images;

namespace Tabibi.API.Controllers
{
    [Route("images")]
    [ApiController]
    public sealed class ImagesController(IWebHostEnvironment environment) : ControllerBase
    {
        [HttpPost("upload")]
        [ProducesResponseType<ImageUploadResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(
            [FromForm] ImageUploadRequestDto requestDto,
            IValidator<ImageUploadRequestDto> validator)
        {
            await validator.ValidateAndThrowAsync(requestDto);

            string imagesFolder = Path.Combine(environment.WebRootPath, "images");
            string uniqueFileName = $"{Guid.CreateVersion7()}{Path.GetExtension(requestDto.File.FileName)}";
            string filePath = Path.Combine(imagesFolder, uniqueFileName);

            using (FileStream fileStream = new(filePath, FileMode.Create))
            {
                await requestDto.File.CopyToAsync(fileStream);
            }

            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{uniqueFileName}";

            ImageUploadResponse response = new(imageUrl);

            return Ok(response);
        }
    }
}
