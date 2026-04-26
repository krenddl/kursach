using AuthApi.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.CustomAtributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int[] _roleId;

        public RoleAuthorizeAttribute(int[] roleId)
        {
            _roleId = roleId;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ContextDb>();

            string? token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(new { error = "Сессия не передана" }) { StatusCode = 401};
                return;
            }

            var user = dbContext.Sessions.Include(x => x.User).FirstOrDefault(x => x.Token == token);
            if (user == null)
            {
                context.Result = new JsonResult(new { error = "Сессия не найдена" }) { StatusCode = 401 };
                return;
            }

            if (!_roleId.Contains(user.User.Role_Id))
            {
                context.Result = new JsonResult(new { error = "Недостаточно прав" }) { StatusCode = 401 };
                return;
            }

            await next();
        }
    }
}
