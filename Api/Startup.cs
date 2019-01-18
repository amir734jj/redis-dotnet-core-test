using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDistributedRedisCache(option =>
            {
                var section = _configuration.GetSection("Redis");

                option.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints =
                    {
                        {section.GetValue<string>("Url"), section.GetValue<int>("Port")}
                    },
                    Password = section.GetValue<string>("Password")
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDistributedCache distributedCache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var (key, value) = ("Name", "Hello world!");

            distributedCache.SetString(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
            
            var cache = distributedCache.GetString(key);

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync((cache == value).ToString());
                distributedCache.Remove(key);
            });
        }
    }
}