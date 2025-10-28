namespace Tabibi.API.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; set; }
        public required string Token { get; set; }
        public required DateTime ExpiresAtUtc { get; set; }

        public required string UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
