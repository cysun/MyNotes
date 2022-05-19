using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using MyNotes.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;
var services = builder.Services;

if (!environment.IsDevelopment())
    builder.WebHost.UseUrls("http://localhost:5004");

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

// Configure Services

services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB (default 30MB)
});

services.AddControllersWithViews();

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie(opt =>
{
    opt.AccessDeniedPath = "/Home/AccessDenied";
    opt.Cookie.MaxAge = TimeSpan.FromDays(90);
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = configuration["OIDC:Authority"];
    options.ClientId = configuration["OIDC:ClientId"];
    options.ClientSecret = configuration["OIDC:ClientSecret"];
    options.ResponseType = "code";
    options.Scope.Add("email");
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Events = new OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/");
            context.HandleResponse();
            return Task.FromResult(0);
        }
    };

    if (environment.IsDevelopment())
    {
        options.RequireHttpsMetadata = false;
    }
});

services.AddAuthorization(options =>
{
    options.AddPolicy("IsOwner", policy =>
        policy.RequireClaim("email", configuration["Application:Owner"]));
});

services.AddRouting(options => options.LowercaseUrls = true);

services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

services.AddScoped<NotesService>();
services.AddScoped<TagsService>();

services.Configure<FilesSettings>(configuration.GetSection("Files"));
services.AddScoped<FilesService>();

// Build App

var app = builder.Build();

// Configure Middleware Pipeline

if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseSerilogRequestLogging();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UsePathBase(configuration["Application:PathBase"]);
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run App

app.Run();
