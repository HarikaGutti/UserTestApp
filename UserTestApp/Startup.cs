using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Http;
using UserTestApp.Models;
using UserTestApp.Services;

namespace UserTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            services.AddHttpClient();

            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };
            services.AddDbContext<DataContext>((sp, options) =>
            {
                if (!string.IsNullOrEmpty(Configuration["ConnectionString"]))
                {
                    options.UseSqlServer(Configuration["ConnectionString"], sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(3).CommandTimeout(30);
                    });
                }
            });
            services.AddHttpClient("windowsAuthClient", c => { })
                .ConfigurePrimaryHttpMessageHandler(() => handler);
            services.AddScoped<ICloudService, CloudService>();
            services.AddSingleton<IAppConfiguration, AppConfiguration>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserTestApp", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserTestApp v1"));
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
