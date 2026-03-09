using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.GitHub;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Components;
using ProjectManager.Services;
using ProjectManager.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Authentication services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddAuthenticationCore();
builder.Services.AddHttpContextAccessor();

// Add GitHub authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Cookies";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]!;
    options.CallbackPath = "/signin-github";
    options.Scope.Add("repo");
    options.SaveTokens = true;

    options.Events = new OAuthEvents
    {
        OnTicketReceived = async context =>
        {
            try
            {
                var db = context.HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>();

                var githubUsername = context.Principal?.Identity?.Name;
                Console.WriteLine($"Checking whitelist for user: {githubUsername}");

                if (githubUsername == null)
                {
                    Console.WriteLine("No GitHub username found");
                    context.HandleResponse();
                    // Prevent ticket creation
                    context.Properties = null;
                    context.Principal = null;
                    await context.HttpContext.SignOutAsync();
                    context.Response.Redirect("/");
                    return;
                }

                var isWhitelisted = await db.WhitelistedUsers
                    .AnyAsync(u => u.GitHubUsername == githubUsername && u.IsActive);

                if (!isWhitelisted)
                {
                    Console.WriteLine($"User {githubUsername} is not whitelisted");
                    context.HandleResponse();
                    // Prevent ticket creation
                    context.Properties = null;
                    context.Principal = null;
                    await context.HttpContext.SignOutAsync();
                    context.Response.Redirect("/");
                    return;
                }

                Console.WriteLine($"User {githubUsername} is whitelisted, proceeding to projects");
                context.ReturnUri = "/app";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in authentication: {ex.Message}");
                context.HandleResponse();
                context.Properties = null;
                context.Principal = null;
                context.Response.Redirect("/");
            }
        },

        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/");
            return Task.CompletedTask;
        }
    };
});

// Add authorization services
builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// Add GitHub service
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" &&
        context.User.Identity != null &&
        context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/app");
    }
    else
    {
        await next();
    }
});

app.Run();