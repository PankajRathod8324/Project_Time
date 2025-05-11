namespace Entities.ViewModel;

    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int? RoleId { get; set; }

        public bool? IsActive { get; set; }
        
        public string RoleName { get; set; } 
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePhoto { get; set; }
        public string? Address { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? Zipcode { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpirytime { get; set; }
    }

