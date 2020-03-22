using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Coravel_Demo_ConsoleApp
{
    class Program
    {
        static volatile int seq = 0;

        static void Main(string[] args)
        {
            var host = new HostBuilder()
                   .ConfigureAppConfiguration((hostContext, configApp) =>
                   {
                       configApp.SetBasePath(Directory.GetCurrentDirectory());
                       configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                       configApp.AddCommandLine(args);
                   })
                   .ConfigureServices((hostContext, services) =>
                   {
                       services.AddSingleton<IDistributedLockFactory>(sp =>
                       {
                           var endPoints = new List<RedLockEndPoint>
                           {
                               new DnsEndPoint("redis", 6379)
                           };
                           return RedLockFactory.Create(endPoints);
                       });



                       services.AddScheduler();

                       services.AddSingleton<RedisMutex>();

                       services.Replace(ServiceDescriptor.Singleton<IMutex>(sp => sp.GetService<RedisMutex>()));
                   })
                   .Build();



            host.Services.UseScheduler(scheduler =>
            {
                scheduler.Schedule(() =>
                {
                    //operacao 1
                    var localSeq = seq++;
                    Console.WriteLine($"Begin {localSeq}");
                    Thread.Sleep(TimeSpan.FromSeconds(4));
                    Console.WriteLine($"\t\t END {localSeq}");
                })
                .EverySeconds(2)
                .PreventOverlapping("redlock:lock:operacao:2")
                ;
            });

            host.Run();

        }


    }



}
