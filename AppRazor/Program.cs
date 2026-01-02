using Services;
using Configuration.Extensions;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;
using Services.Interfaces;
using Encryption;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
