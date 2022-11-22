using Backend.Data;
using Backend.Model;
using Backend.Services;
using JwtSharp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // configure Entity Framework with SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("DataSource=database.sqlite")
            );

            // add membership system for .NET
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            //    services.AddDefa.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
            //.AddEntityFrameworkStores<ApplicationDbContext>();

            // this is to create tokens
            var jwtIssuerOptions = new JwtIssuerOptions()
            {
                Audience = JwtConfiguration.Audience,
                Issuer = JwtConfiguration.Issuer,
                SecurityKey = JwtConfiguration.SigningKey,
                ExpireSeconds = JwtConfiguration.ExpireSeconds
            };
            var jwtIssuer = new JwtIssuer(jwtIssuerOptions);
            services.AddSingleton(jwtIssuer);

            // authentication configuration for .NET
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = JwtConfiguration.GetTokenValidationParameters();
                cfg.Events = new JwtBearerEvents
                {
                    // event for custom responses for not authenticated users
                    OnChallenge = async (context) =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.Headers.Append(
                            HeaderNames.WWWAuthenticate,
                            context.Options.Challenge);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Invalid token" }));
                    }
                };
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Sub] = ClaimTypes.Name;

            // add controllers and services to Dependency Injection Container
            services.AddControllers();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IMobileService, MobileService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ApplicationDbContext dbContext)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();

            SeedUsers(serviceProvider, dbContext).Wait();
        }

        private async Task SeedUsers(IServiceProvider serviceProvider, ApplicationDbContext dbContext)
        {
            // Users and roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var accountHoldersRole = new IdentityRole(Roles.ACCOUNT_HOLDERS.ToString());
            var auditorsRole = new IdentityRole(Roles.AUDITORS.ToString());
            await roleManager.CreateAsync(accountHoldersRole);
            await roleManager.CreateAsync(auditorsRole);

            var billyUser = new User("Billy");
            billyUser.FirstName = "Billy";
            billyUser.LastName = "Hunter";
            billyUser.BirthDate = DateTime.Now.AddYears(-28).AddMonths(-3);
            billyUser.Email = "garage_inc34@gmail.com";
            await userManager.CreateAsync(billyUser, "test");
            await userManager.AddToRoleAsync(billyUser, Roles.ACCOUNT_HOLDERS.ToString());

            var emilyUser = new User("Emily");
            emilyUser.FirstName = "Emily";
            emilyUser.LastName = "White";
            emilyUser.BirthDate = DateTime.Now.AddYears(-42).AddMonths(-7);
            emilyUser.Email = "emily.white@gmail.com";
            await userManager.CreateAsync(emilyUser, "pass");
            await userManager.AddToRoleAsync(emilyUser, Roles.ACCOUNT_HOLDERS.ToString());

            var theAuditor = new User("Michael");
            await userManager.CreateAsync(theAuditor, "secret");
            await userManager.AddToRoleAsync(theAuditor, Roles.AUDITORS.ToString());

            // bank accounts
            var billyAccount = new BankAccount
            {
                Id = "gb86hDWnxR2FIX643bXLkAP9K0jRhlL_Xd9_AYlq5ykw",
                AccountId = "CA-1000-20987",
                CardId = "C3CA7CDA-59F0-4AF3-A10D-C9E29B4AAB70",
                UserId = billyUser.Id,
                UserName = "Billy",
                SSN = "123-45-6789",
                Balance = 5440.50M,
                Audited = false,
                TransactionCode = "4T2524AULM"
            };

            var emilyAccount = new BankAccount
            {
                Id = "QgQEPd-97Jtp8HcCwhTFKAjnDsO9A1rfWmNpdUwFZS6Q",
                AccountId = "CA-1000-20988",
                CardId = "322FAF46-F25E-494D-9015-09DE757B129D",
                UserId = emilyUser.Id,
                UserName = "Emily",
                SSN = "456-78-901",
                Balance = 145700.00M,
                Audited = false,
                TransactionCode = "ZKJJEFXZR1"
            };

            dbContext.Accounts.Add(billyAccount);
            dbContext.Accounts.Add(emilyAccount);
            await dbContext.SaveChangesAsync();
        }
    }
}