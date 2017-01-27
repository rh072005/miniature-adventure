using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace SwashbuckleAspNetApiVersioningExample
{
    public class Startup
    {
        private List<string> _versions;
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

            //https://github.com/aspnet/Mvc/issues/4897#issuecomment-228093609
            //http://stackoverflow.com/questions/36680933/discovering-generic-controllers-in-asp-net-core#answer-37789854

            var mvc = services.AddMvc(c =>            
                c.Conventions.Add(new ApiExplorerGroupPerVersionConvention())                
            );                     
                      
            _versions = SwaggerVersioning.GetAllApiVersions(mvc.PartManager);

            services.AddApiVersioning();                                  

            services.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((version, apiDescription) =>
                {
                    return SwaggerVersioning.SetDocInclusions(version, apiDescription);
                });

                foreach(var version in _versions)
                {
                    c.SwaggerDoc(string.Format($"v{version}"), new Info { Version = version, Title = string.Format($"API V{version}") });
                }            
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();                      

            app.UseSwagger(); 
            app.UseSwaggerUi(c =>
            {                   
                foreach(var version in _versions)
                {
                    c.SwaggerEndpoint(string.Format($"/swagger/v{version}/swagger.json"), string.Format($"V{version} Docs"));
                }
            });
        }
    }
}
