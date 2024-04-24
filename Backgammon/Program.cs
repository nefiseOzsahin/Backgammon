using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<UserService>();//Business içine yazdýðým UserService sýnýfýný diðer kontrollerlarda dependency Injection yapabilmek için bu satýr eklendi.
builder.Services.AddScoped(provider =>
{
    var userService = provider.GetRequiredService<UserService>();
    var users = userService.GetUsersAsStringList(); // Get the list of users as strings
    return new BackgammonPairing(users);
});


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<BackgammonContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BackgammonCookie";
    options.LoginPath = new PathString("/Home/Index");
});
builder.Services.AddDbContext<BackgammonContext>(opt =>
{
    //opt.UseSqlServer("Data Source=DESKTOP-75H6AC3\\SQLEXPRESS;Initial Catalog=BackgammonDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
    //opt.UseSqlServer("Server=77.245.159.27\\MSSQLSERVER2019;Database=chouette_;User Id=nefise;Password=Erkin2019!;Connect Timeout=30;Encrypt=False;");
    opt.UseSqlServer("Server=77.245.159.27\\MSSQLSERVER2019;Database=ankaratavl_;User Id=nefise;Password=Erkin2019!;Connect Timeout=30;Encrypt=False;");

});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();

// Superuser creation
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
});
var serviceProvider = builder.Services.BuildServiceProvider();
var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
var createSuperUser = Task.Run(async () =>
{
    if (await roleManager.FindByNameAsync("Admin") == null)
    {
        var adminRole = new AppRole { Name = "Admin" };
        await roleManager.CreateAsync(adminRole);
    }

    if (await userManager.FindByNameAsync("erkin") == null)
    {
        var user = new AppUser { UserName = "erkin" };
        var result = await userManager.CreateAsync(user, "Genclerbirligi1923!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            // Handle errors if user creation fails
            Console.WriteLine("Failed to create superuser:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
});
createSuperUser.Wait(); // Wait for the superuser creation task to complete

// Add other services as needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{Controller}/{Action}",
        defaults: new { Controller = "Home", Action = "Index" }
        );
});

app.Run();
