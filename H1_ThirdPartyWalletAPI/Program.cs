using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var tcs = new TaskCompletionSource();
            //var sigintReceived = false;
            //Console.CancelKeyPress += (sender, e) =>
            //{
            //    e.Cancel = true;
            //    Console.WriteLine("Received SIGINT (Ctrl+C)");
            //    tcs.SetResult();
            //    sigintReceived = true;
            //};
            //AssemblyLoadContext.Default.Unloading += ctx =>
            //{
            //    if (!sigintReceived)
            //    {
            //        Console.WriteLine("Received SIGTERM");
            //        tcs.SetResult();
            //    }
            //    else
            //    {
            //        Console.WriteLine("@AssemblyLoadContext.Default.Unloading¡AHandle SIGINT¡AIgnore SIGTERM");
            //    }
            //};
            //AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            //{
            //    if (!sigintReceived)
            //    {
            //        Console.WriteLine("Received SIGTERM");
            //        tcs.SetResult();
            //    }
            //    else
            //    {
            //        Console.WriteLine("@AppDomain.CurrentDomain.ProcessExit¡AHandle SIGINT¡AIgnore SIGTERM");
            //    }
            //};
            System.Threading.ThreadPool.SetMinThreads(256, 256);

            var hostBuilder = WebApplication.CreateBuilder(args);

            hostBuilder.Host.UseSerilog((context, logger) => {
                logger
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
            });

            var startup = new Startup(hostBuilder.Configuration, hostBuilder.Environment);
            startup.ConfigureServices(hostBuilder.Services);

            var host = hostBuilder.Build();

            startup.Configure(host);

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Host created.");
            try
            {
                host.Run();
            }
            catch(Exception ex)
            {
                logger.LogCritical("Host Excetpion : {ex}", ex);
            }
            finally
            {
                logger.LogCritical("Host done.");
            }
        }
    }
}
