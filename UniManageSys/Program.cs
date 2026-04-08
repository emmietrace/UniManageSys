using Microsoft.AspNetCore.Identity.UI.Services;
using UniManageSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();
// Register our dummy email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IMatriculationService, MatriculationService>();
builder.Services.AddScoped<ICourseRegistrationService, CourseRegistrationService>();
builder.Services.AddScoped<ICourseAssignmentService, CourseAssignmentService>();
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();

var app = builder.Build();
// --- Role Seeding Block ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // The 7 pillars of our uni system
    string[] roles =
    {
        "SuperAdmin", "Registrar", "HOD", "Lecturer",
        "Student", "FinanceOfficer", "ExamOfficer"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // --- Create a SECOND SuperAdmin ---
    string adminEmail2 = "sysadmin@unisystem.edu";
    var adminUser2 = await userManager.FindByEmailAsync(adminEmail2);

    if (adminUser2 == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail2,
            Email = adminEmail2,
            FirstName = "System",
            LastName = "AdminTwo",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "SuperAdmin");
        }
        else
        {
            // Catching errors just in case this one fails too!
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create second admin user: {errors}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    //.WithStaticAssets();

app.MapRazorPages();

app.Run();
