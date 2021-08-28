using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Quartz;
using Shortener.Service.Extensions;
using Shortener.Service.Jobs;

namespace Shortener.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // Add selfhost port for demo
                    webBuilder.UseUrls("http://localhost:8000");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionScopedJobFactory();
                        //
                        // Add periodical/scheduled jobs
                        // Time definition is in appsettings
                        // https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontriggers.html
                        // Daily - MON-SUN on 08:00 CET for the previous day => Cron expression: "0 0 8 ? * MON-SUN"
                        q.AddJobAndTrigger<SendDailyNotification>(hostContext.Configuration);
                        // Weekly - MON on 08:01 CET for the past week => Cron expression: "0 1 8 ? * MON"
                        q.AddJobAndTrigger<SendWeeklyNotification>(hostContext.Configuration);
                        // Monthly - first MON on the month for the past month => Cron expression: "0 2 8 * * MON#1"
                        q.AddJobAndTrigger<SendMonthlyNotification>(hostContext.Configuration);
                    });

                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);
                });
    }
}