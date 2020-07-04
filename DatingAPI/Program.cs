using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatingAPI.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var host =CreateWebHostBuilder(args).Build().Run();
            //using (var scope = host.Services.CreateScope())
            //{
            //    var service = scope.ServiceProvider;
            //    try
            //    {
            //        var context = service.GetRequiredService<DataContext>();
            //        context.Database.Migrate();
            //        Seed.SeedUsers(context);
            //    }
            //    catch(Exception ex)
            //    {

            //    }
            //}
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
