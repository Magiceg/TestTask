using System.ComponentModel.DataAnnotations;

namespace TestTask.ViewModel
{
    public class UserRoleViewModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        [Required]
        public string Email { get; set; }
        public int[] RolesId { get; set; }
    }
}
