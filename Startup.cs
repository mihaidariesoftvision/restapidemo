namespace Library.API
{
    using System.Collections.Generic;
    using AspNetCoreRateLimit;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Library.API.Services;
    using Library.API.Entities;
    using Microsoft.EntityFrameworkCore;
    using Library.API.Helpers;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Hosting;
    using AutoMapper;
    using Library.API.Formatters;
    using System.Text.Json;
    using Microsoft.AspNetCore.ResponseCaching;
    using Microsoft.OpenApi.Models;
    using Library.API.Filter;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class Startup
    {
        // ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public IConfiguration Configuration { get; }

#pragma warning restore CA2211 // Non-constant fields should not be visible

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
#pragma warning disable CA1822 // Mark members as static
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
        {
            services
                .AddMvc(setupAction =>
                {
                    setupAction.RespectBrowserAcceptHeader = true;
                    setupAction.ReturnHttpNotAcceptable = true;
                    setupAction.OutputFormatters.Add(new CustomJsonOutputFormatter(new JsonSerializerOptions()));
                    setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                    setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter(new MvcOptions()));

                    // add xml formatter for xml custom media types

                    var xmlDataContractSerializerInputFormatter = setupAction.InputFormatters.OfType<XmlDataContractSerializerInputFormatter>().FirstOrDefault();
                    //xmlDataContractSerializerInputFormatter?.SupportedMediaTypes.Add(MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathXmlInput);

                    // add json formatter for json custom media types

                    var jsonInputFormatter = setupAction.InputFormatters.OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();
                    jsonInputFormatter?.SupportedMediaTypes.Add(MediaTypes.ApplicationVndMihaiAuthorFullJsonInput);
                    //jsonInputFormatter?.SupportedMediaTypes.Add(MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonInput);

                    var jsonOutputFormatter = setupAction.OutputFormatters.OfType<CustomJsonOutputFormatter>().FirstOrDefault();
                    jsonOutputFormatter?.SupportedMediaTypes.Add(MediaTypes.ApplicationVndMihaiHateoasJsonOutput);
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                })
                // .AddNewtonsoftJson(
                //     options =>
                // {
                //     //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //     options.UseCamelCasing(true);
                // })
                ;
                //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();
            services.AddTransient<IResourceUri, ResourceUri>();

            #region Auto Mapper Configurations

            services.AddSingleton(
                new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new ModelMappingProfile());
                }).CreateMapper());

            #endregion

            #region response caching

            if (bool.Parse(Configuration["useCaching"]))
            {
                services.AddResponseCaching(configureOptions =>
                {
                    //configureOptions
                });

                services.AddHttpCacheHeaders(
                    expirationModelOptions => expirationModelOptions.MaxAge = 600, // in seconds, 5 minutes
                    validationModelOptions => validationModelOptions.MustRevalidate = true);
            }

            #endregion

            #region Rate Limiting Services

            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>((options) => options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*", // all APIs
                    Limit = 1000, // 1000 requests
                    Period = "5m" // 5 minutes
                },
                new RateLimitRule
                {
                    Endpoint = "*", // all APIs
                    Limit = 200, // 200 requests
                    Period = "10s" // 10 seconds
                }
            });

            // add a caching mechanism to store the limits and the counters
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            #endregion

            #region swagger generator

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("v1",
                new OpenApiInfo { Title = "Library API", Version = "v1" });

                setupAction.ResolveConflictingActions(apiDescriptions =>
                {
                    // little hack applied here, as routing by media type isn't supported in OpenAPI
                    return apiDescriptions.First();
                });

                setupAction.OperationFilter<CreateAuthorOperationFilter>();

                //setupAction.GeneratePolymorphicSchemas();
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable CA1822 // Mark members as static
        public void Configure(
#pragma warning restore CA1822 // Mark members as static
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILogger<Startup> logger,
            LibraryContext libraryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder => appBuilder.Run(async context =>
                {
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();

                    if (exceptionHandler != null)
                    {
                        logger.LogError(500, exceptionHandler.Error, exceptionHandler.Error.Message);
                    }

                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Generic error message!").ConfigureAwait(true);
                }));
            }

            libraryContext.EnsureSeedDataForContext();

            #region Rate Limiting Middleware

            app.UseIpRateLimiting(); // must be first in the pipeline

            #endregion

            #region response caching

            if (bool.Parse(Configuration["useCaching"]))
            {
                app.UseResponseCaching();

                app.UseHttpCacheHeaders();

                app.Use(async (context, next) =>
                {
                    var responseCachingFeature = context.Features.Get<IResponseCachingFeature>();
                    if (responseCachingFeature != null)
                    {
                        responseCachingFeature.VaryByQueryKeys = new[] { "*" };
                    }

                    await next().ConfigureAwait(true);
                });
            }

            #endregion

            // these need to sit after caching components hook made above
            // so the controllers won't be hit anymore if caching can serve the request
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region swagger config

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
                c.RoutePrefix = string.Empty;
            });

            #endregion
        }
    }
}