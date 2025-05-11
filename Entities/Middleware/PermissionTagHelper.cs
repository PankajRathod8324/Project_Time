using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Entities.Middleware;


[HtmlTargetElement("button", Attributes = "permission")]
[HtmlTargetElement("a", Attributes = "permission")]
[HtmlTargetElement("div", Attributes = "permission")]
[HtmlTargetElement("li",Attributes = "permission")]
public class PermissionTagHelper : TagHelper
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionTagHelper(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
         Console.WriteLine(" PermissionTagHelper Initialized");
    }

    [HtmlAttributeName("permission")]
    public string Permission { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (!user.Identity.IsAuthenticated)
        {
            Console.WriteLine("User is not authenticated. Hiding element.");
            output.SuppressOutput();
            return;
        }

        string policyName = $"{Permission}Policy";
        Console.WriteLine($"Checking UI policy: {policyName}");

        var authResult = await _authorizationService.AuthorizeAsync(user, null, policyName);

        if (authResult.Succeeded)
        {
            Console.WriteLine($"UI: User has permission for {policyName}");
        }
        else
        {
            Console.WriteLine($"UI: User DOES NOT have permission for {policyName}. Hiding element.");
            output.SuppressOutput();
        }
    }

}