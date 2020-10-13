using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MyNotes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Due to recent enforcement of "SameSite=None; Secure" by the major browsers
                    // (https://blog.chromium.org/2019/10/developers-get-ready-for-new.html),
                    // AIS must run with HTTPS even in dev environment. Use the following command
                    // to create a trusted dev certificate:
                    //      dotnet dev-certs https --trust
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                        webBuilder.UseStartup<Startup>();
                    else
                        webBuilder.UseStartup<Startup>().UseUrls("http://localhost:5004");
                })
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration));
    }
}
