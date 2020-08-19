using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddLog4Net();//添加log4Net
            })
            .UseKestrel(op =>
            {
                op.ListenAnyIP(8083);
            }).UseStartup<Startup>();
    }
}
