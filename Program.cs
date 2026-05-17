using ExpenseTracker.Data;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database (SQLite)
builder.Services.AddDbContext<ExpenseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity (without default UI)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ExpenseDbContext>();

// ✅ Blazor + Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpContextAccessor();

// ✅ Custom Services
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Redirect old Identity endpoints to your Blazor pages
app.MapGet("/Identity/Account/Login", context =>
{
    context.Response.Redirect("/authentication/login");
    return Task.CompletedTask;
});

app.MapGet("/Identity/Account/Register", context =>
{
    context.Response.Redirect("/authentication/register");
    return Task.CompletedTask;
});

app.MapGet("/Identity/Account/Logout", context =>
{
    context.Response.Redirect("/authentication/logout");
    return Task.CompletedTask;
});

app.MapGet("/Account/Logout", context =>
{
    context.Response.Redirect("/authentication/logout");
    return Task.CompletedTask;
});

app.MapPost("/authentication/login", async (HttpContext context,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var rememberMe = form["rememberMe"].ToString().Equals("on", StringComparison.OrdinalIgnoreCase);
    var returnUrl = GetSafeReturnUrl(form["returnUrl"].ToString());

    if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                context.Response.Redirect(returnUrl);
                return;
            }
        }
    }

    context.Response.Redirect("/authentication/login?error=Invalid%20login%20attempt.");
});

app.MapPost("/authentication/register", async (HttpContext context,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password) ||
        string.IsNullOrWhiteSpace(confirmPassword))
    {
        context.Response.Redirect("/authentication/register?error=All%20fields%20are%20required.");
        return;
    }

    if (password != confirmPassword)
    {
        context.Response.Redirect("/authentication/register?error=Passwords%20do%20not%20match.");
        return;
    }

    var user = new IdentityUser
    {
        UserName = email,
        Email = email
    };

    var result = await userManager.CreateAsync(user, password);
    if (result.Succeeded)
    {
        await signInManager.SignInAsync(user, isPersistent: false);
        context.Response.Redirect("/");
        return;
    }

    var error = Uri.EscapeDataString(string.Join(" ", result.Errors.Select(e => e.Description)));
    context.Response.Redirect($"/authentication/register?error={error}");
});

app.MapPost("/authentication/logout", async (HttpContext context,
    SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
    context.Response.Redirect("/authentication/login");
});

static string GetSafeReturnUrl(string? returnUrl)
{
    if (!string.IsNullOrWhiteSpace(returnUrl) &&
        returnUrl.StartsWith("/", StringComparison.Ordinal) &&
        !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
        !returnUrl.Contains("://", StringComparison.Ordinal))
    {
        return returnUrl;
    }

    return "/";
}

// ✅ Redirect root ("/") to login if not authenticated
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" && context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/authentication/login");
        return;
    }
    await next();
});


// ❌ Removed app.MapRazorPages(); so Identity’s default UI isn’t served
// app.MapRazorPages();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
