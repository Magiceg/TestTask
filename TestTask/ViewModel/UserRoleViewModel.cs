using System.ComponentModel.DataAnnotations;

namespace TestTask.ViewModel
{
    public class UserRoleViewModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        [Required]
        public string Email { get; set; }
#pragma warning disable CS1591 // There is no XML comment for an open visible type or member
        public int[] RolesId { get; set; }
#pragma warning restore CS1591 // There is no XML comment for an open visible type or member
    }
}
