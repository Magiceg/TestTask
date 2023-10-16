using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.Controllers;
using TestTask.Data;
using TestTask.Model;
using TestTask.Services.Interfaces;

namespace TestTask.Services
{
	public class RoleService : IRoleService
	{
		private readonly AppDbContext _context;
		private readonly ILogger<UsersController> _logger;
		/// <summary>
		/// Get the context from the database
		/// </summary>
		public RoleService(AppDbContext context, ILogger<UsersController> logger)
		{
			_context = context;
			_logger = logger;
		}
		public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
		{
			try
			{
				if (_context.Roles == null)
				{
					return new NotFoundResult();
				}

				var roles = await _context.Roles.ToListAsync();
				return new OkObjectResult(roles);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while getting roles.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		public async Task<ActionResult<Role>> GetRoleById(int id)
		{
			try
			{
				if (_context.Roles == null)
				{
					return new NotFoundResult();
				}

				var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);

				if (role == null)
				{
					return new NotFoundResult();
				}

				return new OkObjectResult(role);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while getting a role by ID.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		public async Task<IActionResult> PutRole(int id, [FromBody] Role model)
		{
			try
			{
				if (id != model.Id)
				{
					return new BadRequestResult();
				}

				var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
				role.Name = model.Name;

				try
				{
					await _context.SaveChangesAsync();
					return new OkResult();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!RoleExists(id))
					{
						return new NotFoundResult();
					}
					else
					{
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while updating a role.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		public async Task<ActionResult<Role>> PostRole([FromBody] Role role)
		{
			try
			{
				if (_context.Roles == null)
				{
					return new ObjectResult("Entity set 'AppDbContext.Roles' is null.")
					{
						StatusCode = StatusCodes.Status500InternalServerError
					};
				}

				await _context.Roles.AddAsync(role);
				await _context.SaveChangesAsync();

				return new CreatedAtActionResult("GetRole", "Role", new { id = role.Id }, role);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while creating a role.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		public async Task<IActionResult> DeleteRole(int id)
		{
			try
			{
				if (_context.Roles == null)
				{
					return new NotFoundResult();
				}

				var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
				if (role == null)
				{
					return new NotFoundResult();
				}

				_context.Roles.Remove(role);
				await _context.SaveChangesAsync();

				return new NoContentResult();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting a role.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		private bool RoleExists(int id)
		{
			return (_context.Roles?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
