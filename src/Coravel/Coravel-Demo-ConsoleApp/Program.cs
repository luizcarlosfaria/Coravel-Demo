using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                       services.AddScheduler();
                   })
                   .Build();

            var endPoints = new List<RedLockEndPoint>
            {
                new DnsEndPoint("redis", 6379)
            };
            var redlockFactory = RedLockFactory.Create(endPoints);

            host.Services.UseScheduler(scheduler =>
            {
                scheduler.Schedule(() =>
                {
                    //operacao 1
                  

                    using (var redisLock = redlockFactory.CreateLock(
                        resource: "lock:operacao:1",
                        expiryTime: TimeSpan.FromSeconds(10),
                        waitTime: TimeSpan.FromSeconds(20),
                        retryTime: TimeSpan.FromSeconds(2))
                        )
                    {
                        var localSeq = seq++;

                        if (redisLock.IsAcquired)
                        {
                            Console.WriteLine($"Begin {localSeq} - {redisLock.IsAcquired}");
                            Thread.Sleep(TimeSpan.FromSeconds(4));
                            Console.WriteLine($"\t\t END {localSeq}");
                        }
                        else
                            Console.WriteLine($"\t\t {localSeq} não executada!");
                    }
                })
                .EverySeconds(2);
            });

            host.Run();

        }


    }



}
