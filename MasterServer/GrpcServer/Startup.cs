using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MasterServer.DarkRift;
using Microsoft.Extensions.Configuration;
using SessionKeyManager;
using System.IO;
using MasterServer.Config;
using MasterServer.DarkRift.Authentication;

namespace MasterServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Path.Combine(env.ContentRootPath, "ConfigFiles"));

            builder.AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("jwtsettings.json", false, true)
                .AddJsonFile("darkriftsettings.json", false, true)
                .AddJsonFile("serveraddresses.json",false,true);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtConfig = new JwtConfig();
            Configuration.GetSection("Jwt").Bind(jwtConfig);

            services.AddGrpc();
            services.AddControllers();
            services.AddHttpContextAccessor();

            services.AddAuthentication().AddJwtAuthentication(jwtConfig.Key,jwtConfig.Issuer);
            services.AddAuthorization();

            ConfigureBindings(services);
        }

        private void ConfigureBindings(IServiceCollection services) 
        {
            services.AddSingleton(Configuration.GetSection("DarkRiftServer").Get<DarkRiftServerConfig>());
            services.AddSingleton(Configuration.GetSection("ServerAddresses").Get<ServerAddresses>());
            services.AddSingleton<DRClientManager>();
            services.AddSingleton<DRAuthenticator>();
            services.AddSingleton<DRClientHelper>();
            services.AddSingleton<DRCommunicator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
