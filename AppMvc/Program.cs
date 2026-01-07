using Services;
using Configuration.Extensions;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;
using Services.Interfaces;
using Encryption;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Configuration.AddSecrets(builder.Environment);

builder.Services.AddEncryptions(builder.Configuration);
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddUserBasedDbContext();

builder.Services.AddVersionInfo();
builder.Services.AddEnvironmentInfo();

builder.Services.AddScoped<AdminDbRepos>();
builder.Services.AddScoped<AddressesDbRepos>();
builder.Services.AddScoped<FriendsDbRepos>();
builder.Services.AddScoped<LoginDbRepos>();
builder.Services.AddScoped<PetsDbRepos>();
builder.Services.AddScoped<QuotesDbRepos>();

builder.Services.AddScoped<JwtEncryptions>();

builder.Services.AddScoped<IAdminService, AdminServiceDb>();
builder.Services.AddScoped<IFriendsService, FriendsServiceDb>();
builder.Services.AddScoped<IAddressesService, AddressesServiceDb>();
builder.Services.AddScoped<IPetsService, PetsServiceDb>();
builder.Services.AddScoped<IQuotesService, QuotesServiceDb>();
builder.Services.AddScoped<ILoginService, LoginService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    //https://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security
    //https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
    app.UseHsts();
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

