using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Oracle.ManagedDataAccess.Client;
using WebApplication1.Models;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //自定义配置文件目录
            var basePath = Directory.GetCurrentDirectory();
            CustomConfiguration = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
        }

        public IConfiguration Configuration { get; }
        public static IHostingEnvironment HostingEnvironment { get; internal set; }
        public IConfigurationRoot CustomConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataBaseContext>(opt => opt.UseOracle(Configuration.GetConnectionString("OracleConnectStrings")));
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            #region 注册跨域服务 并指定允许请求的数据源
            //注册跨域服务 并指定允许请求的数据源
            IConfiguration allowUrlSection = CustomConfiguration.GetSection("Address");
            string allowurl = "";
            if (allowUrlSection != null)
            {
                allowurl = allowUrlSection["allowUrl"];
            }
            string[] allowArray = allowurl.Split(";", StringSplitOptions.RemoveEmptyEntries);
            services.AddCors(Action => Action.AddPolicy("AllowSpecificOrigin", policy =>
            policy.WithOrigins(allowArray)
            .AllowAnyHeader()
            .AllowAnyMethod() //如果不加此方法 默认只允许Get方式跨域请求
            // .AllowCredentials() // 指定处理cookie
            ));
            #endregion
            //解决ViewBag的中文编码问题
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddDistributedMemoryCache();
            services.AddSession(o=> { o.IOTimeout = TimeSpan.FromSeconds(3200); });
            services.AddHttpClient();//将HttpClient注入进来
            services.AddMvc().AddJsonOptions(option =>
            {
                option.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            HostingEnvironment = env;
            app.UseHttpsRedirection();
            app.UseFastReport();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            #region 启用跨域
            app.UseCors("AllowSpecificOrigin");
            #endregion
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Index}/{id?}");
            });
        }
    }
}
