using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using Io.Autometa.Lobby.WebServer.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;

namespace Io.Autometa.Lobby.WebServer
{
    public class Startup
    {
        private const string ApiTitle = "Lobby.Autometa.io";
        private const string ApiVersion = "v1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddServerTiming();

            // TODO: Swap with services.AddMvcCore().AddApiExplorer();
            services.AddMvc();

            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersion, new Info { Title = ApiTitle, Version = ApiVersion });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // wrap up lobby exceptions so they are pretty
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Lobby.Server.LobbyException ex)
                {
                    context.Response.StatusCode = ex.HttpCode;
                    context.Response.Headers["Location"] = "lobby.autometa.io";
                    context.Response.Body = new MemoryStream();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    byte[] data = Encoding.UTF8.GetBytes(ex.Message);
                    await context.Response.Body.WriteAsync(data, 0, data.Length);
                }
            });

            // Accept forwarded-IP headers. This allows functionality
            // for users behind NAT, but will also allow for IP spoofing.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseMvc();

            // Expose swagger middleware for development
            app.UseSwagger();

            // Should be near the end
            app.UseServerTiming();
        }
    }
}