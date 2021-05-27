using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScarletMVC.Services;

namespace ScarletMVC
{
    public class GoogleAuthentication {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string Authority { get; set; }
    }
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var googleAuthentication = Configuration.GetSection("GoogleAuthentication").Get<GoogleAuthentication>();

            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "GoogleOIDC";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect("GoogleOIDC", options => {
                options.Authority = googleAuthentication.Authority; 
                options.ClientId = googleAuthentication.ClientId;
                options.ClientSecret = googleAuthentication.ClientSecret;
                options.CallbackPath = googleAuthentication.CallbackPath;
                options.SaveTokens = true;
                options.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "109979528005478075270")
                        {
                            var claim = new Claim(ClaimTypes.Role, "Admin");
                            var claimIdentity = context.Principal.Identity as ClaimsIdentity;
                            claimIdentity.AddClaim(claim);
                        }
                    }
                };
            });

            services.Configure<OtherSettings>(Configuration.GetSection($"OtherSettings:{env.EnvironmentName}"));
            services.Configure<IdentityServerSettings>(Configuration.GetSection($"IdentityServerSettings:{env.EnvironmentName}"));
            services.AddTransient<ITokenService, TokenService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
