using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DIBAdminAPI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.Title = "DibWeb";
            CreateHostBuilder(args).Build().Run();
            return 0;

        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                         .ConfigureWebHostDefaults(webBuilder => {
                             webBuilder.UseStartup<Startup>();

                         });

    }
}
