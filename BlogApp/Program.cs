using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete;
using BlogApp.Data.Concrete.EfCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BlogContext>(options=>{options.UseSqlite(builder.Configuration["ConnectionStrings:sql_connection"]);
});

// Add scoped services
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Must be called before UseAuthorization
app.UseAuthorization();

SeedData.TestVerileriniDoldur(app);

app.UseEndpoints(endpoints =>
{
    // Default route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=StartPage}/{action=Index}/{id?}");

    // Route for details page
    endpoints.MapControllerRoute(
        name: "startpage_details",
        pattern: "StartPage/Details/{url}",
        defaults: new { controller = "StartPage", action = "Details" });

    // Route for filtering by tag
    endpoints.MapControllerRoute(
        name: "startpage_by_tag",
        pattern: "StartPage/tag/{tag}",
        defaults: new { controller = "StartPage", action = "Index" });

    // Route for user login
    endpoints.MapControllerRoute(
        name: "user_login",
        pattern: "Users/Login",
        defaults: new { controller = "Users", action = "Login" });

    // Route for user profile
    endpoints.MapControllerRoute(
        name: "user_profile",
        pattern: "Users/Profile",
        defaults: new { controller = "Users", action = "Profile" });
});

app.Run();
