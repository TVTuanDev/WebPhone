using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebPhone.Attributes
{
    public class AppAuthorizeAttribute : TypeFilterAttribute
    {
        public string RoleName { get; set; }
        public AppAuthorizeAttribute(string roleName = "Guest") : base(typeof(AppAuthorizeFilter))
        {
            RoleName = roleName;
            Arguments = new object[] { RoleName };
        }
    }

    public class AppAuthorizeFilter : IAuthorizationFilter
    {
        private string RoleName { get; set; }
        private readonly IHttpContextAccessor _contextAccessor;

        public AppAuthorizeFilter
            (
                string roleName,
                IHttpContextAccessor contextAccessor
            )
        {
            RoleName = roleName;
            _contextAccessor = contextAccessor;
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra action của controller có [AllowAnonymous] hay không?
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
            if (allowAnonymous) return;

            var httpContext = _contextAccessor.HttpContext;
            // User chua dang nhap
            if (!httpContext!.User.Identity!.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            if (!CanAccessToAction(context.HttpContext))
                context.Result = new ForbidResult();
        }

        private bool CanAccessToAction(HttpContext httpContext)
        {
            if (RoleName == "Guest") return true;

            var listRoleNameContext = httpContext.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            var listRoleNameAttr = RoleName.Split(",").ToList();
            if (listRoleNameAttr.Count > 1)
            {
                foreach (var roleName in listRoleNameAttr)
                {
                    if (listRoleNameContext.Contains(roleName.Trim())) return true;
                }
            }
            else
            {
                if(listRoleNameContext.Contains(RoleName.Trim())) return true;
            }

            return false;
        }
    }
}
