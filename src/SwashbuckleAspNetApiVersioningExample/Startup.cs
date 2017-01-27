using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwashbuckleAspNetVersioningShim;

namespace SwashbuckleAspNetApiVersioningExample
{
    public class Startup
    {
        private IMvcBuilder _mvcBuilder;
        private SwaggerVersioner _swaggerVersioning = new SwaggerVersioner();

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
            //https://github.com/aspnet/Mvc/issues/4897#issuecomment-228093609
            //http://stackoverflow.com/questions/36680933/discovering-generic-controllers-in-asp-net-core#answer-37789854

            _mvcBuilder = services.AddMvc(c =>            
                c.Conventions.Add(new ApiExplorerGroupPerVersionConvention(_swaggerVersioning))                
            );                     

            services.AddApiVersioning();                                
            services.AddSwaggerGen(c =>
            {
                _swaggerVersioning.ConfigureSwaggerGen(c, _mvcBuilder.PartManager);
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
                _swaggerVersioning.ConfigureSwaggerUi(c, _mvcBuilder.PartManager);
            });
        }
    }
}
