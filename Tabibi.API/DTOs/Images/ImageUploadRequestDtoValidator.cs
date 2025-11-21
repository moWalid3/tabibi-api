using FluentValidation;

namespace Tabibi.API.DTOs.Images
{
    public sealed class ImageUploadRequestDtoValidator : AbstractValidator<ImageUploadRequestDto>
    {
        public ImageUploadRequestDtoValidator()
        {
            RuleFor(i => i.File)
                .NotNull()
                .WithMessage("Image is required")
                .Must(file =>
                {
                    string[] allwedExtensions = [".png", ".jpg", ".jpeg"];
                    string extension = Path.GetExtension(file.FileName).ToLower();
                    return allwedExtensions.Contains(extension);
                })
                .WithMessage("Only .png, .jpg, .jpeg extensions are allowed.")
                .Must(file => file.Length <= 500 * 1024)
                .WithMessage("Image size must be less than 500 KB.");
        }
    }
}
