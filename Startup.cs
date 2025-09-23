using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace APISietemasdereservas
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager"));
            });

            services.AddCors();
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartBoundaryLengthLimit = int.MaxValue;
                options.MultipartHeadersCountLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddMemoryCache();
            services.AddControllers().AddXmlSerializerFormatters();
            services.AddDistributedMemoryCache();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "http://localhost:61768/",
                        ValidAudience = "http://localhost:61768/",
                        TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890ABCDEF")),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("estesessunssecretossuperslargosdes32sbytess2024s")),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options =>
            {
                options.WithOrigins("*");
                options.AllowAnyHeader();
                options.AllowAnyMethod();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages(
                "application/json", "Message: Se ha denegado la autorizacion para esta solicitud, Estado code: {0} ");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsApi");
            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();
            IConfigurationRoot GetConfiguration()
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                return builder.Build();

            }
            var configuracion = GetConfiguration();
            
            string imagePath = configuracion.GetSection("PathImages").Value;

            if (Directory.Exists(imagePath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(imagePath),
                    RequestPath = "/imagenes"
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
