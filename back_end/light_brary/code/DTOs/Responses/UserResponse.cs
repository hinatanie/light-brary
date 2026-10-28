using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record UserResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }

        internal static UserResponse FromEntity(User? user, IEnumerable<string> roles)
        {
            if (user is null) return new UserResponse();

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber!,
                Address = user.Address,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
