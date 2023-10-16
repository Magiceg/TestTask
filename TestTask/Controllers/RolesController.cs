using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TestTask.Data;
using TestTask.Model;
using TestTask.Services;
using TestTask.Services.Interfaces;

namespace TestTask.Controllers
{
	/// <summary>
	/// Controller for managing roles.
	/// </summary>
	[Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

		/// <summary>
		/// Get roles
		/// </summary>
		/// <returns>List of roles</returns>
		[HttpGet]
		[SwaggerOperation(Summary = "Get roles")]
		[SwaggerResponse(StatusCodes.Status200OK, "Roles list")]
		public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
			return await _roleService.GetRoles();
		}

		/// <summary>
		/// Get a role by Id
		/// </summary>
		/// <param name="id">Role Id</param>
		/// <returns>Role details</returns>
		[HttpGet("{id}")]
		[SwaggerOperation(Summary = "Get a role by Id")]
		[SwaggerResponse(StatusCodes.Status200OK, "The role details")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Role not found")]
		public async Task<ActionResult<Role>> GetRole(int id)
        {
			return await _roleService.GetRoleById(id);
		}

		/// <summary>
		/// Update a role by Id
		/// </summary>
		/// <param name="id">Role Id</param>
		/// <param name="model">Role details to update</param>
		/// <returns>Result of the update operation</returns>
		[HttpPut("{id}")]
		[SwaggerOperation(Summary = "Update a role by Id")]
		[SwaggerResponse(StatusCodes.Status200OK, "Role updated")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Role not found")]
		public async Task<IActionResult> PutRole(int id, [FromBody] Role model)
        {
            return await _roleService.PutRole(id, model);
        }

		/// <summary>
		/// Create a new role
		/// </summary>
		/// <param name="role">Role details</param>
		/// <returns>Created role</returns>
		[HttpPost]
		[SwaggerOperation(Summary = "Create a new role")]
		[SwaggerResponse(StatusCodes.Status201Created, "Role created", typeof(Role))]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request")]
		public async Task<ActionResult<Role>> PostRole([FromBody] Role role)
        {
            return await _roleService.PostRole(role);
        }

		/// <summary>
		/// Delete a role by Id
		/// </summary>
		/// <param name="id">Role Id</param>
		/// <returns>Result of the delete operation</returns>
		[HttpDelete("{id}")]
		[SwaggerOperation(Summary = "Delete a role by Id")]
		[SwaggerResponse(StatusCodes.Status204NoContent, "Role deleted")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Role not found")]
		public async Task<IActionResult> DeleteRole(int id)
        {
            return await _roleService.DeleteRole(id);
        }

       
    }
}
