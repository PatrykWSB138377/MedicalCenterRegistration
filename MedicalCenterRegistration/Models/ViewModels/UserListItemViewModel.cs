using System;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
