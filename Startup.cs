using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ATTM_API.Services;
using ATTM_API.Helpers;
using ATTM_API.SignalRHub;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;

namespace ATTM_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            // requires using Microsoft.Extensions.Options
            services.Configure<ATTMDatabaseSettings>(Configuration.GetSection(nameof(ATTMDatabaseSettings)));
            services.AddSingleton<IATTMDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ATTMDatabaseSettings>>().Value);
            services.Configure<ATTMAppSettings>(Configuration.GetSection(nameof(ATTMAppSettings)));
            services.AddSingleton<IATTMAppSettings>(appSP =>
                appSP.GetRequiredService<IOptions<ATTMAppSettings>>().Value);

            services.AddSingleton<CategoryService>();
            services.AddSingleton<TestSuiteService>();
            services.AddSingleton<TestGroupService>();
            services.AddSingleton<TestCaseService>();
            services.AddSingleton<TestEnvironmentService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<KeywordService>();
            services.AddSingleton<TestProjectService>();
            services.AddSingleton<TestProjectExplorerService>();
            services.AddSingleton<TestAUTService>();
            services.AddSingleton<TestClientService>();
            services.AddSingleton<DevQueueService>();
            services.AddSingleton<DevRunRecordService>();
            services.AddSingleton<RegressionService>();
            services.AddSingleton<RegressionTestService>();
            services.AddSingleton<RegressionRunRecordService>();
            services.AddSingleton<GridFSBucketService>();
            services.AddControllers()
                .AddNewtonsoftJson(options => options.UseMemberCasing());
            services.AddControllers();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
                    Title = "ATTM API",
                    Version = "v1",
                });
            });
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ATTM_FE API"));
            }
            loggerFactory.AddLog4Net();
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = exception.Message,
                });
            }));
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x.WithOrigins("http://localhost:8080")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MonitoringHub>("/monitoring");
            });
        }
    }
}
