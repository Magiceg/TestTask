using Microsoft.AspNetCore.Mvc;
using TestTask.Model;
using TestTask.ViewModel;

namespace TestTask.Services.Interfaces
{
	public interface IUserService
	{
		Task<ActionResult<IEnumerable<User>>> GetUsers(int page, int pageSize, string sortBy, string sortOrder, string filter);

		Task<ActionResult<User>> CreateUser([FromBody] UserRoleViewModel vm);

		Task<ActionResult<User>> GetUser(int id);

		Task<IActionResult> UpdateUser(int id, UserRoleViewModel vm);

		Task<IActionResult> AddUserRole(int userId, int roleId);

		Task<IActionResult> DeleteUser(int id);
	}
}

