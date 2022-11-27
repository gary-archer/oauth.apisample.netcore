namespace SampleApi.Plumbing.Security
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;

    /*
     * This prevents the custom authentication handler running for anonymous endpoints
     */
    public sealed class CustomAuthorizeFilter : AuthorizeFilter
    {
        public CustomAuthorizeFilter(AuthorizationPolicy policy)
            : base(policy)
        {
        }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (this.EndpointRequiresAuthorization(context))
            {
                await base.OnAuthorizationAsync(context);
            }
        }

        private bool EndpointRequiresAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(
                em => em.GetType() == typeof(AllowAnonymousAttribute)))
            {
                System.Console.WriteLine("*** Has anonymous");
                return false;
            }

            return true;
        }
    }
}
