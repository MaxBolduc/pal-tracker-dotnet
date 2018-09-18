using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.Info;
using Swashbuckle.AspNetCore.Swagger;

namespace PalTracker
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
            services.AddMvc();
            
            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));

            services.AddSingleton(sp => new WelcomeMessage(
               Configuration.GetValue<string>("WELCOME_MESSAGE", "WELCOME_MESSAGE not configured.")
           ));

           services.AddSingleton(sp => new CloudFoundryInfo(
               Configuration.GetValue<string>("PORT", "PORT not configured."),
               Configuration.GetValue<string>("MEMORY_LIMIT", "MEMORY_LIMIT not configured."),
               Configuration.GetValue<string>("CF_INSTANCE_INDEX", "CF_INSTANCE_INDEX not configured."),
               Configuration.GetValue<string>("CF_INSTANCE_ADDR", "CF_INSTANCE_ADDR not configured.")
           ));

           services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();
           services.AddSingleton<IHealthContributor, TimeEntryHealthContributor>();
           services.AddSingleton<IOperationCounter<TimeEntry>, OperationCounter<TimeEntry>>();
           services.AddSingleton<IInfoContributor, TimeEntryInfoContributor>();
           services.AddSingleton<IInfoContributor, TestInfoContributor>();
            
           // Register the Swagger generator, defining 1 or more Swagger documents
           services.AddSwaggerGen(c =>
           {
               c.SwaggerDoc("v1", new Info { Title = "Pal Tracker API", Version = "v1" });
           });
            
           services.AddCloudFoundryActuators(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pal Tracker API V1");
            });
            
            app.UseMvc();
            app.UseCloudFoundryActuators();
        }
    }
}
