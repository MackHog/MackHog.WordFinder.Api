using Microsoft.Extensions.DependencyInjection;
using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Domain;
using System.Security.Claims;
using SwashbuckleAspNetVersioningShim;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;
using System.IO;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Builder;
using GlobalExceptionHandler.WebApi;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace WebApp.Config
{
    public static class StartupExtensions
    {
        public static void AddCompositionRoot(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(appSettings);
            services.AddSingleton(appSettings);
            services.AddSingleton<IWordService, InMemoryWordService>();
        }

        public static void AddAuth(this IServiceCollection services, IHostingEnvironment env)
        {
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddAuthorization(options =>
            {
                if (env.IsDevelopment())
                {
                    options.AddPolicy(Constants.Policy.Read, policy => policy.RequireAssertion(e => true));
                }
                else
                    options.AddPolicy(Constants.Policy.Read, policy => policy.RequireClaim(ClaimTypes.Role, Constants.Policy.Read));
            });
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                c.ConfigureSwaggerVersions(provider, $"{Constants.Swagger.Title}");
                c.DescribeAllEnumsAsStrings();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Basic", new ApiKeyScheme
                {
                    In = "Header",
                    Description = "Please enter credentials",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Basic", Enumerable.Empty<string>() },
                });
            });
        }

        public static void UseSwaggerVersioning(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.ConfigureSwaggerVersions(provider, new SwaggerVersionOptions
                {
                    DescriptionTemplate = Constants.Swagger.Title + " v{0}",
                    RouteTemplate = "/swagger/{0}/swagger.json"
                });
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.RoutePrefix = string.Empty;
            });
        }

        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            app.UseGlobalExceptionHandler(x =>
            {
                x.ContentType = "application/json";
                x.MessageFormatter(s => JsonConvert.SerializeObject(new
                {
                    Message = "An error occurred whilst processing your request"
                }));
                x.ForException<ArgumentNullException>().ReturnStatusCode(StatusCodes.Status400BadRequest)
                    .UsingMessageFormatter((ex, context) => JsonConvert.SerializeObject(new { ex.Message }));
                x.ForException<ArgumentException>().ReturnStatusCode(StatusCodes.Status400BadRequest)
                    .UsingMessageFormatter((ex, context) => JsonConvert.SerializeObject(new { ex.Message }));
                x.ForException<Exception>().ReturnStatusCode(StatusCodes.Status500InternalServerError)
                   .UsingMessageFormatter((ex, context) => JsonConvert.SerializeObject(new { ex.Message }));
            });

            return app;
        }
    }
}
