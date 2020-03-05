using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DatingApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(x=> {
                x.UseLazyLoadingProxies();
                x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
                
            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            // SQL SERVER CONTEXT IN PRODUCTION
            services.AddDbContext<DataContext>(x=> {
                x.UseLazyLoadingProxies();
                x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"));
            });
            
            // MY SQL CONTEXT IN PRODUCTION
            // services.AddDbContext<DataContext>(x=> {
            //     x.UseLazyLoadingProxies();
            //     x.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            // });
        
            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // using addidentitycore because that allows us to use our jwt token
            // if we use addidentity it sets up with cookies
            IdentityBuilder builder = services.AddIdentityCore<User>(opt =>
            {
                // using options so that we can set weak passwords, not something to do in production
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            });

            // take the new builder and build the services for it
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            // this will add these services so that the tables in our db are created to handle identity 
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            // configure authentication scheme we'r egoing to use
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                });

            // creating policies to protect endpoints
            services.AddAuthorization(options => 
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
                options.AddPolicy("VipOnly", policy => policy.RequireRole("VIP"));
            });

            services.AddControllers(options => 
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                options.Filters.Add(new AuthorizeFilter(policy));

            }).AddNewtonsoftJson(opt => 
            {
                // temp, ignore the looping exception which is caused by user object referencing photos and photos
                // object referencing users
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });


            // add cors service for middleware
            services.AddCors();
            // map the cloudinary settings (api key/secret) to the cloudinary settings helper
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            // using nuget package "AutoMapper.Extensions.Microsoft.DependencyInjection" to link dto's to models
            services.AddAutoMapper(typeof(DatingRepository).Assembly);

            services.AddScoped<IDatingRepository, DatingRepository>();

            services.AddScoped<LogUserActivity>();
            
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // logs exceptions and then puts them in an alternative pipeline
                // then run a terminal command ".run" to get the context back, store the error status code in context
                // then create a variable error to store the feature of the error
                // then if error is not null write error message into http
                // we will also write our own extension to response stored in Helpers folder
                app.UseExceptionHandler(builder => {
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if(error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();

            app.UseRouting();

            // put authentication here in the app pipeline
            app.UseAuthentication();
            app.UseAuthorization();

            // add rules to allow cors headers these are default cors that allow any origin
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // tells kestrel root file to look for standard html default files in the wwwroot folder
            // where we directed our NG Build for the spa
            app.UseDefaultFiles();
            // and now tell the server to use these static files
            app.UseStaticFiles();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // since the routes is handled in angular, when the server runs an angular route off the 5000 port, 
                // it needs to be directed to the route, this is a specialized route endpoint
                // anything its not aware of thats not an api end point will fallback to teh index pages routes
                // in the wwwroute folder
                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}
