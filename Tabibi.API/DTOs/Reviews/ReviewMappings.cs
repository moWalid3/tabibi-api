using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Reviews
{
    public static class ReviewMappings
    {
        public static ReviewDto ToDto(this Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                PatientName = review.Patient?.Name,
                PatientAvatar = review.Patient?.AvatarUrl,
                CreatedAt = review.CreatedAt
            };
        }

        public static Review ToEntity(
            this CreateReviewDto dto,
            string doctorId,
            string patientId)
        {
            return new Review
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                DoctorId = doctorId,
                PatientId = patientId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
