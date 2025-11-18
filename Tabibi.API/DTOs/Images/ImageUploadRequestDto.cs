using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tabibi.API.DTOs.Images
{
    [ValidateNever]
    public sealed record ImageUploadRequestDto(IFormFile File);
}
