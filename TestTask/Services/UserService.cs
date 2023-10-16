using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TestTask.Controllers;
using TestTask.Data;
using TestTask.Model;
using TestTask.Services.Interfaces;
using TestTask.Services.UserMethod;
using TestTask.ViewModel;

namespace TestTask.Services
{
    public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly ILogger<UsersController> _logger;
		/// <summary>
		/// Get the context from the database
		/// </summary>
		public UserService(AppDbContext context, ILogger<UsersController> logger)
		{
			_context = context;
			_logger = logger;
		}


		public async Task<ActionResult<IEnumerable<User>>> GetUsers(int page, int pageSize, string sortBy, string sortOrder, string filter)
		{
			try
			{
				if (_context.Users == null)
				{
					return new NotFoundResult();
				}

				// Database query for users
				var query = _context.Users.Include(x => x.UserRoles).ThenInclude(y => y.Role).AsQueryable();

				// Dynamic filtering using the method from UserFilters
				query = UserFilters.ApplyFilters(query, filter);

				// Dynamic Sorting 
				query = UserSorting.ApplySorting(query, sortBy, sortOrder);

				// Pagination
				var totalItems = await query.CountAsync();
				var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

				if (page < 1) page = 1;
				if (page > totalPages) page = totalPages;

				var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

				var result = new
				{
					TotalItems = totalItems,
					TotalPages = totalPages,
					Page = page,
					PageSize = pageSize,
					Users = users
				};

				// Implementation of logging
				_logger.LogInformation("Successfully retrieved the list of users.");

				return new OkObjectResult(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving the list of users.");
				return new StatusCodeResult(500);
			}
		}
		public async Task<ActionResult<User>> GetUser(int id)
		{
			try
			{
				if (_context.Users == null)
				{
					return new NotFoundResult();
				}

				var user = await _context.Users
					.Include(x => x.UserRoles)
					.ThenInclude(y => y.Role)
					.FirstOrDefaultAsync(x => x.Id == id);

				if (user == null)
				{
					return new NotFoundObjectResult("User not found.");
				}

				return new OkObjectResult(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while retrieving user by ID.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}
		public async Task<ActionResult<User>> CreateUser([FromBody] UserRoleViewModel vm)
		{
			try
			{
				if (_context.Users == null)
				{
					/* If the Users dataset in the database context is null,
				     * return the problem with the corresponding message.
				     */
					return new ObjectResult("Entity set 'AppDbContext.Users' is null.")
					{
						StatusCode = StatusCodes.Status400BadRequest
					};
				}

				// Checking for required fields (Name, Age, Email).
				var validationContext = new ValidationContext(vm, null, null);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(vm, validationContext, validationResults, true))
				{
					return new BadRequestObjectResult(validationResults);
				}
				// Checking the uniqueness of the Email.
				if (_context.Users.Any(x => x.Email == vm.Email))
				{
					return new BadRequestObjectResult("Email is already in use.");
				}
				// Checking the age for a positive number.
				if (vm.Age <= 0)
				{
					return new BadRequestObjectResult("The age cannot be less than zero");
				}

				// Creating a new user and adding it to the database context.
				var user = new User()
				{
					Name = vm.Name,
					Age = vm.Age,
					Email = vm.Email
				};
				foreach (var roleId in vm.RolesId)
				{
					// Checking the existence of a role by role Id. 
					var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
					if (role == null)
					{
						// If the role is not found, we return the Bad Request error.
						return new BadRequestObjectResult($"Role with Id {roleId} not found.");
					}
					// Add role to user.
					user.UserRoles.Add(new UserRole()
					{
						User = user,
						RoleId = roleId
					});
				}
				_context.Users.Add(user);
				await _context.SaveChangesAsync();
				return new CreatedResult("Your create user location URI", user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while creating a new user.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}

				
		}

		public async Task<IActionResult> UpdateUser(int id, UserRoleViewModel vm)
		{
			// Search for a user by ID
			var user = await _context.Users
				.Include(x => x.UserRoles)
				.ThenInclude(y => y.Role)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (user == null)
			{
				// If the user with the specified ID is not found, return the NotFound error.
				return new NotFoundObjectResult("User not found");
			}
			// Updating user information from the UserRoleViewModel
			user.Name = vm.Name;
			user.Age = vm.Age;
			user.Email = vm.Email;

			var existingRoleIds = user.UserRoles.Select(x => x.RoleId).ToList();
			var newRoleIds = vm.RolesId.ToList();
			// Defining roles to add and remove
			var rolesToAdd = newRoleIds.Except(existingRoleIds).ToList();
			var rolesToRemove = existingRoleIds.Except(newRoleIds).ToList();

			// Deleting roles that are not listed in the new role list.
			foreach (var roleIdToRemove in rolesToRemove)
			{
				var roleToRemove = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleIdToRemove);
				if (roleToRemove != null)
				{
					user.UserRoles.Remove(roleToRemove);
				}
			}

			// Adding new Roles
			foreach (var roleIdToAdd in rolesToAdd)
			{
				var roleToAdd = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleIdToAdd);
				if (roleToAdd != null)
				{
					user.UserRoles.Add(new UserRole()
					{
						Role = roleToAdd
					});
				}
			}

			_context.Users.Update(user);

			try
			{
				await _context.SaveChangesAsync();
				return new OkResult();
			}
			catch (DbUpdateConcurrencyException)
			{
				// Handling concurrency conflict error when saving data.
				if (!UserExists(id))
				{
					// If the user was deleted by another request, we return the Not Found error.
					return new NotFoundObjectResult("User not found");
				}
				else
				{
					throw;
				}
			}
		}

		public async Task<IActionResult> AddUserRole(int userId, int roleId)
		{
			try
			{
				// Get the user by user Id.
				var user = await _context.Users
					.Include(x => x.UserRoles)
					.ThenInclude(y => y.Role)
					.FirstOrDefaultAsync(x => x.Id == userId);

				if (user == null)
				{
					return new NotFoundObjectResult("User not found");
				}

				// Get the role by RoleId.
				var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);

				// Check whether the user already has such a role.
				if (role == null)
				{
					return new NotFoundObjectResult("Role not found");
				}

				// Check whether the user already has such a role.
				if (user.UserRoles.Any(ur => ur.RoleId == roleId))
				{
					// If the role already exists for the user, return a BadRequest with a message.
					return new BadRequestObjectResult("User already has this role");
				}

				user.UserRoles.Add(new UserRole()
				{
					Role = role
				});

				_context.Users.Update(user);

				try
				{
					await _context.SaveChangesAsync();
					return new OkResult();
				}
				catch (DbUpdateConcurrencyException)
				{
					//If an error occurred while saving, we check whether the user exists.
					if (!UserExists(userId))
					{
						return new NotFoundObjectResult("User not found");
					}
					else
					{
						// If another error occurs,     throw an exception.
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while adding a role to the user.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		public async Task<IActionResult> DeleteUser(int id)
		{
			try
			{
				if (_context.Users == null)
				{
					return new NotFoundResult();
				}
				var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
				if (user == null)
				{
					return new NotFoundResult();
				}

				_context.Users.Remove(user);
				await _context.SaveChangesAsync();

				return new NoContentResult();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting the user.");
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}



		private bool UserExists(int id)
		{
			return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
