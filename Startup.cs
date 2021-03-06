﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using StundenplanImport.Model;
using Microsoft.Extensions.FileProviders;

namespace StundenplanImport
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            /*
            using (var stream = new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
            {
                var secrets = GoogleClientSecrets.Load(stream).Secrets;

                app.UseGoogleAuthentication(new GoogleOptions()
                {
                    ClientId = secrets.ClientId,
                    ClientSecret = secrets.ClientSecret,
                    Scope = { CalendarService.Scope.Calendar }, 
                    SignInScheme = "Session",
                    AuthenticationScheme = "Google",
                });
            }*/

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");

                /*
                routes.MapRoute(name: "login",
                    template: "{controller=Login}/{action=Login}/{id?}");
                */

                routes.MapRoute(name: "export",
                    template: "{controller=Export}/{action=ICalendar}");
            });
        }
    }
}
