using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SmileShop.Authorization.Clients;
using SmileShop.Authorization.Configurations;
using SmileShop.Authorization.Data;
using SmileShop.Authorization.Filters;
using SmileShop.Authorization.Helpers;
using SmileShop.Authorization.Middlewares;
using SmileShop.Authorization.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SmileShop.Authorization
{
    public class Startup
    {
        private const string _projectName = "Please Rename your Project.";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ErrorFilter));
                options.Filters.Add(new ValidateModelFilter());
            })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; //ReferenceLoopHandling : กันไม่ให้ dto ซ้อน dto;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // NullValueHandling : ข้อมูลที่มีค่าเป็น null จะไม่แสดงใน response;
                });

            services.AddHttpContextAccessor();
            services.AddResponseCaching();

            //------Allow Origins------
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            //------End: Allow Origins------

            //------HealthChecks------
            services.AddHealthChecks().AddDbContextCheck<AppDBContext>(tags: new[] { "ready" });
            //------End: HealthChecks------

            //------AutoMapper------
            services.AddAutoMapper(typeof(Startup));
            //------End: AutoMapper------

            //------DBContext------
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //------End: DBContext------

            //------Options ------
            services.Configure<Jwt>(Configuration.GetSection("jwt"));
            services.Configure<ServiceURL>(Configuration.GetSection("ServiceURL"));
            //--------------------

            //------RestShape Client------
            services.AddSingleton<ShortLinkClient>();
            services.AddSingleton<SendSmsClient>();
            //------End: RestShape Client------

            //------OData------
            services.AddOData();
            //------End: OData------

            //------Swagger------
            AddSwagger(services);
            //------End: Swagger------

            //------Authentication------
            AddAuthentication(services);
            //------End: Authentication------

            //------Service------
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAuthService, AuthService>();
            //------End: Service------

            AddFormatters(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //------Logging Failed Requests to Serilog Using Middleware ------
            /*app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });*/

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            //reference = https://blog.bitscry.com/2019/01/14/logging-failed-requests-using-middleware/
            //------End: Logging Failed Requests to Serilog Using Middleware ------

            //------Swagger------
            app.UseSwagger(config =>
            {
                config.PreSerializeFilters.Add((swagger, httpRequest) =>
                {
                    swagger.Servers.Clear();
                });
            });

            app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", _projectName));
            //------End: Swagger------

            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthentication();

            app.UseAuthorization();

            //------Allow Origins------
            app.UseCors("MyPolicy");
            //------End: Allow Origins------

            //------HealthChecks------
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponseReadiness,
                    Predicate = (check) => check.Tags.Contains("ready")
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponseLiveness,
                    Predicate = (check) => !check.Tags.Contains("ready")
                });
            });
            //------End: HealthChecks------

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region Method

        /// <summary>
        /// Add Authentication
        /// </summary>
        /// <param name="services"></param>
        private void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                            .AddJwtBearer(options =>

                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = false,
                                    ValidateAudience = false,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                                    ClockSkew = TimeSpan.Zero,
                                    RequireExpirationTime = true
                                }
                            );
        }

        /// <summary>
        /// Add Swagger
        /// </summary>
        /// <param name="services"></param>
        private static void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = _projectName, Description = "Project Template Description ..." });

                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                    });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Add Formatter for OData with swagger
        /// </summary>
        /// <param name="services"></param>
        public void AddFormatters(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }

                foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            }
            );
        }

        #endregion Method
    }
}