using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<UserService>();//Business i�ine yazd���m UserService s�n�f�n� di�er kontrollerlarda dependency Injection yapabilmek i�in bu sat�r eklendi.
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<BackgammonContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BackgammonCookie";
    options.LoginPath = new PathString("/Home/Index");
});
builder.Services.AddDbContext<BackgammonContext>(opt =>
{
    //opt.UseSqlServer("Data Source=DESKTOP-75H6AC3\\SQLEXPRESS;Initial Catalog=BackgammonDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
    opt.UseSqlServer("Server=77.245.159.27;Database=chouette_;User Id=nefise;Password=Erkin2019!;Connect Timeout=30;Encrypt=False;");

});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();



// Add other services as needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
