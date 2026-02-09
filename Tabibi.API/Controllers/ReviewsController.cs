using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Reviews;
using Tabibi.API.Entities;
using Tabibi.API.Entities.Enums;
using Tabibi.API.Services;

namespace Tabibi.API.Controllers
{
    [Route("reviews")]
    [ApiController]
    public sealed class ReviewsController(
        AppDbContext dbContext,
        NotificationService notificationService) : ControllerBase
    {
        [Authorize(Roles = Roles.Patient)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReview(
            CreateReviewDto createReviewDto,
            IValidator<CreateReviewDto> validator)
        {
            await validator.ValidateAndThrowAsync(createReviewDto);

            string? patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking? booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == createReviewDto.BookingId &&
                                          b.PatientId == patientId &&
                                          b.Status == BookingStatus.Completed);

            if (booking == null)
            {
                return BadRequest("Invalid booking. You can only review your own completed appointments.");
            }

            bool alreadyReviewed = await dbContext.Reviews
                .AnyAsync(r => r.BookingId == createReviewDto.BookingId);

            if (alreadyReviewed)
            {
                return BadRequest("You have already reviewed this appointment.");
            }

            Review review = createReviewDto.ToEntity(booking.DoctorId, patientId!);

            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                booking.DoctorId,
                "New Review Received",
                $"A patient gave you {createReviewDto.Rating} stars.",
                NotificationType.System,
                review.Id
            );

            return Ok();
        }


        [Authorize(Roles = Roles.Patient)]
        [HttpGet("doctor/{doctorId}")]
        [ProducesResponseType<PaginationResult<ReviewDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDoctorReviews(
            string doctorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            IOrderedQueryable<Review> query = dbContext.Reviews
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.CreatedAt);

            int totalCount = await query.CountAsync();

            List<ReviewDto> reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => r.ToDto())
                .ToListAsync();

            PaginationResult<ReviewDto> result = new()
            {
                Items = reviews,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }


        [Authorize(Roles = Roles.Doctor)]
        [HttpGet("me")]
        [ProducesResponseType<DoctorReviewsPageDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReviews(
            int page = 1,
            int pageSize = 10,
            int? rating = null)
        {
            string? doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allReviewsQuery = dbContext.Reviews.Where(r => r.DoctorId == doctorId);

            var ratingGroup = await allReviewsQuery
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            int totalReviews = ratingGroup.Sum(x => x.Count);

            double averageRating = totalReviews > 0
                ? await allReviewsQuery.AverageAsync(r => r.Rating)
                : 0;

            ReviewStatsDto stats = new(
                AverageRating: Math.Round(averageRating, 1),
                TotalReviews: totalReviews,
                FiveStarCount: ratingGroup.FirstOrDefault(x => x.Rating == 5)?.Count ?? 0,
                FourStarCount: ratingGroup.FirstOrDefault(x => x.Rating == 4)?.Count ?? 0,
                ThreeStarCount: ratingGroup.FirstOrDefault(x => x.Rating == 3)?.Count ?? 0,
                TwoStarCount: ratingGroup.FirstOrDefault(x => x.Rating == 2)?.Count ?? 0,
                OneStarCount: ratingGroup.FirstOrDefault(x => x.Rating == 1)?.Count ?? 0
            );

            var listQuery = allReviewsQuery
                .Include(r => r.Patient)
                .AsQueryable();

            if (rating.HasValue)
            {
                listQuery = listQuery.Where(r => r.Rating == rating.Value);
            }

            List<ReviewDto> pagedReviews = await listQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => r.ToDto())
                .ToListAsync();

            PaginationResult<ReviewDto> reviewsResult = new()
            {
                Items = pagedReviews,
                Page = page,
                PageSize = pageSize,
                TotalCount = rating.HasValue ? await listQuery.CountAsync() : totalReviews
            };

            return Ok(new DoctorReviewsPageDto(stats, reviewsResult));
        }
    }
}
