using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using MyNotes.Services;
using Serilog;

namespace MyNotes
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(opt =>
            {
                opt.AccessDeniedPath = "/Home/AccessDenied";
            })
            //.AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = Configuration["OIDC:Authority"];
                options.ClientId = Configuration["OIDC:ClientId"];
                options.ClientSecret = Configuration["OIDC:ClientSecret"];
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

                if (Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsOwner", policy =>
                    policy.RequireClaim("email", Configuration["Application:Owner"]));
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

            services.AddScoped<NotesService>();
            services.AddScoped<TagsService>();
            services.AddScoped<FilesService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
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

            app.UsePathBase(Configuration["Application:PathBase"]);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
