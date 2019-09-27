using DistributedCacheExercise.Extensions;
using DistributedCacheExercise.Interfaces;
using DistributedCacheExercise.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DistributedCacheExercise
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
            services.AddGlobalMongoCacheDatabase(Configuration);
            services.AddSingleton<ITextKeyValueCacheService<string>, InMemoryKeyValueCacheService<string>>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var hasCacheLoaded = false;

            // Reload the cache to get the recent fresh data.
            app.Use(async (context, next) =>
            {
                if (!hasCacheLoaded)
                {
                    var globalCacheService = context.RequestServices.GetService<ITextKeyValueCacheService<string>>();
                    await globalCacheService.ReloadAsync();
                    hasCacheLoaded = true;
                }

                await next.Invoke();
            });

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}