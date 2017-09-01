using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BitcoinApi.Filters;
using Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using OffchainMonitorApi.Middleware;
using OffchainMonitorApi.Binder;
using SqlliteRepositories;
using LkeServices.Triggers;
using System.Reflection;
using LkeServices.Triggers.Bindings;
using OffchainMonitorApi.Functions;

namespace OffchainMonitorApi
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        //public void ConfigureServices(IServiceCollection services)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(Configuration,
                "OffchainMonitorSettings");

            services.AddMvc(o =>
            {
                o.Filters.Add(new HandleAllExceptionsFilterFactory());
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "OffchainMonitor_Api", Version = "v1" });
                options.DescribeAllEnumsAsStrings();

                //Determine base path for the application.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;

                //Set the comments path for the swagger json and ui.
                var xmlPath = Path.Combine(basePath, "OffchainMonitorApi.xml");
                options.IncludeXmlComments(xmlPath);
            });

            var builder = new SqlliteBinder().Bind(settings);
            builder.Populate(services);
            builder.RegisterType<TimerTriggerBinding>();
            builder.RegisterType<QueueTriggerBinding>();
            builder.RegisterType<CommitmentBroadcastCheck>();
            var container = builder.Build();
            var triggerHost = new TriggerHost(new AutofacServiceProvider(container));
            triggerHost.ProvideAssembly(GetType().GetTypeInfo().Assembly);
            triggerHost.StartAndBlock();

            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<GlobalErrorHandlerMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "OffchainMonitor_Api"));
        }
    }
}
