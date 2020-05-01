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

namespace HangfireScheduler
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
            services.AddControllers();

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

            RecurringJob.AddOrUpdate(() => Console.WriteLine("Hello"), Cron.Minutely, TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate(() => Method(), Cron.Hourly, TimeZoneInfo.Local);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Method()
        {
            var restClient = new RestClient();
            var request = new RestRequest(@"http://pugachserver/WebScraper/api/ProductWatcher/price?productId=2");

            var response = await restClient.ExecutePostAsync(request);

            if (!response.IsSuccessful)
                throw new HttpRequestException("Запрос бы не успешен");
        }
    }
}
