using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using TestTask.Data;
using TestTask.Model;
using TestTask.Services.Filtering;
using TestTask.Services.Sorting;
using TestTask.ViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestTask.Controllers
{   /// <summary>
    /// Controller for user management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Users", "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;


        /// <summary>
        /// get the context from the database
        /// </summary>
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a list of users
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="sortBy">Sorting field</param>
        /// <param name="sortOrder">Sorting direction</param>
        /// <param name="filter">Filter</param>
        /// <returns>A list of users</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Get a list of users")]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery] int page = 1,           // Page number
            [FromQuery] int pageSize = 10,      // Page Size
            [FromQuery] string sortBy = "Id",   // Sorting field
            [FromQuery] string sortOrder = "asc",// Sorting direction
            [FromQuery] string filter = "")     // Filter
        {
            if (_context.Users == null)
            {
                return NotFound();
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

            return Ok(result);
        }


        /// <summary>
        /// Get a user by Id
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a user by Id")]
        [SwaggerResponse(StatusCodes.Status200OK, "The user details")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users
                        .Include(x => x.UserRoles)
                        .ThenInclude(y => y.Role)
                        .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }


        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="vm">User model</param>
        /// <returns>Updated user</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update user")]
        [SwaggerResponse(StatusCodes.Status200OK, "User updated")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        public async Task<IActionResult> PutUser(int id, UserRoleViewModel vm)
        {
            // Search for a user by ID
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(y => y.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                // If the user with the specified ID is not found, return the NotFound error.
                return NotFound("User not found");
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
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handling concurrency conflict error when saving data.
                if (!UserExists(id))
                {
                    // If the user was deleted by another request, we return the Not Found error.
                    return NotFound("User not found");
                }
                else
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="vm">User model</param>
        /// <returns>Created User</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Create User")]
        [SwaggerResponse(StatusCodes.Status201Created, "User created", typeof(User))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request")]
        public async Task<ActionResult<User>> PostUser([FromBody] UserRoleViewModel vm)
        {
            if (_context.Users == null)
            {
                /* If the Users dataset in the database context is null,
                 * return the problem with the corresponding message.
                 */
                return Problem("Entity set 'AppDbContext.Users'  is null.");
            }

            // Checking for required fields (Name, Age, Email).
            var validationContext = new ValidationContext(vm, null, null);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(vm, validationContext, validationResults, true))
            {
                return BadRequest(validationResults);
            }
            // Checking the uniqueness of the Email.
            if (_context.Users.Any(x => x.Email == vm.Email))
            {
                return BadRequest("Email is already in use.");
            }
            // Checking the age for a positive number.
            if (vm.Age <= 0)
            {
                return BadRequest("The age cannot be less than zero");
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
                    return BadRequest($"Role with Id {roleId} not found.");
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

            return Ok();
        }


        /// <summary>
        /// Add a user role
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="roleId">Role Id</param>
        /// <returns>Added a user role</returns>
        [HttpPost("AddUserRole/{userId}")]
        [SwaggerOperation(Summary = "Add a user role")]
        [SwaggerResponse(StatusCodes.Status200OK, "User role added")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or role not found")]
        public async Task<IActionResult> AddUserRole(int userId, int roleId)
        {
            // Get the user by user Id.
            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(y => y.Role)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get the role by RoleId.
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);

            // Check whether the user already has such a role.
            if (role == null)
            {
                return NotFound("Role not found");
            }

            // Check whether the user already has such a role.
            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
            {
                // If the role already exists for the user, return a BadRequest with a message.
                return BadRequest("User already has this role");
            }

            user.UserRoles.Add(new UserRole()
            {
                Role = role
            });

            _context.Users.Update(user);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                //If an error occurred while saving, we check whether the user exists.
                if (!UserExists(userId))
                {
                    return NotFound("User not found");
                }
                else
                {
                    // If another error occurs,     throw an exception.
                    throw;
                }
            }
        }


        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Deleted user</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete user")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "User deleted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
