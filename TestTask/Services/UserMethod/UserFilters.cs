using TestTask.Model;

namespace TestTask.Services.UserMethod
{
    public static class UserFilters
    {
        public static IQueryable<User> ApplyFilters(IQueryable<User> query, string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                // filtering by all roles
                query = query.Where(u =>
                    u.Name.Contains(filter) ||
                    u.Age.ToString().Contains(filter) ||
                    u.UserRoles.Any(ur => ur.Role.Name.Contains(filter)) ||
                    u.Email.Contains(filter));
            }

            return query;
        }
    }
}
