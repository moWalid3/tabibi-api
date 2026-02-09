using FluentValidation;

namespace Tabibi.API.DTOs.Reviews
{
    public sealed class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty()
                .WithMessage("Booking ID is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5 stars.");

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}
