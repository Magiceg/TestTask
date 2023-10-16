using Microsoft.AspNetCore.Mvc;
using TestTask.Model;

namespace TestTask.Services.Interfaces
{
	public interface IRoleService
	{
		Task<ActionResult<IEnumerable<Role>>> GetRoles();

		Task<ActionResult<Role>> GetRoleById(int id);

		Task<IActionResult> PutRole(int id, [FromBody] Role model);

		Task<ActionResult<Role>> PostRole([FromBody] Role role);

		Task<IActionResult> DeleteRole(int id);
	}
}
