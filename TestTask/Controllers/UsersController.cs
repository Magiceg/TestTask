using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestTask.Model;
using TestTask.Services.Interfaces;
using TestTask.ViewModel;

namespace TestTask.Controllers
{   /// <summary>
	/// Controller for user management.
	/// </summary>
	[Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Users", "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
            return await _userService.GetUsers(page, pageSize, sortBy, sortOrder, filter);

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
			return await _userService.GetUser(id);
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
			return await _userService.UpdateUser(id, vm);
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
			return await _userService.CreateUser(vm);
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
			return await _userService.AddUserRole(userId, roleId);
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
			return await _userService.DeleteUser(id);
		}

        
    }
}
