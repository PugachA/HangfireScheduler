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
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Common;
using RestSharp;
using System.Threading;
using System.Linq.Expressions;
using System.Net.Http;
using Hangfire.Storage;

namespace HangfireScheduler
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true)
                .AddConfiguration(configuration);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient(provider => Configuration);

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,

                })
                .WithJobExpirationTimeout(TimeSpan.FromDays(365))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })
                );

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IRecurringJobManager backgroundJobs, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard();

            //var manager = new RecurringJobManager();

            //RecurringJob.AddOrUpdate("Beru1", () => Method(@"http://pugachserver/WebScraper/api/ProductWatcher/price?productId=1"), "0 30 * ? * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("Beru2", () => Method(@"http://pugachserver/WebScraper/api/ProductWatcher/price?productId=2"), "0 0 * ? * *", TimeZoneInfo.Local);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Method(string requestUrl)
        {
            var restClient = new RestClient();
            var request = new RestRequest(requestUrl);

            var response = await restClient.ExecutePostAsync(request);

            if (!response.IsSuccessful)
                throw new HttpRequestException("Запрос бы не успешен");
        }
    }
}
