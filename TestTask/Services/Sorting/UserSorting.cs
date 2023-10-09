using System.Linq;
using System.Linq.Expressions;
using TestTask.Model;

namespace TestTask.Services.Sorting
{
    public static class UserSorting
    {
        public static IQueryable<User> ApplySorting(IQueryable<User> query, string sortField, string sortOrder)
        {
            // "parameter" is used to create sort expressions 
            var parameter = Expression.Parameter(typeof(User), "x");
            /* "property" represents access to a property
             * of an object of type "user" with the name 
             * specified in "sortField"
             */
            var property = Expression.Property(parameter, sortField);   
            /* this lambda expression is passed to methods
             * "OrderBy" и "OrderByDescending" to preform
             * sorting
             */
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sortOrder == "asc" ? "OrderBy" : "OrderByDescending";
            var types = new[] { typeof(User), property.Type };
            //using reflection to get a method "OrderBy" and "OrderByDescending"
            var method = typeof(Queryable).GetMethods().Single(
                x => x.Name == methodName && x.IsGenericMethodDefinition && x.GetParameters().Length == 2);

            var genericMethod = method.MakeGenericMethod(types);

            return (IQueryable<User>)genericMethod.Invoke(null, new object[] { query, lambda });
        }
    }
}
