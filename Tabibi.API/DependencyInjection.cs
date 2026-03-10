using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Stripe;
using System.Text;
using Tabibi.API.Common;
using Tabibi.API.Common.Sorting;
using Tabibi.API.Configurations;
using Tabibi.API.Database;
using Tabibi.API.DTOs.AdminDoctors;
using Tabibi.API.DTOs.AdminPatients;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.DTOs.Doctors;
using Tabibi.API.Entities;
using Tabibi.API.Middlewares;
using Tabibi.API.Services;
using Tabibi.EmailService;

namespace Tabibi.API
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            builder.Services.AddOpenApi();

            return builder;
        }

        public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
        {
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            return builder;
        }

        public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"), sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Meta);
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped<TokenProvider>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<IFcmNotificationService, FcmNotificationService>();

            builder.Services.AddTransient<DataShapingProvider>();

            builder.Services.AddHostedService<BookingCleanupService>();

            builder.Services.AddSignalR();

            return builder;
        }

        public static WebApplicationBuilder AddSortServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<SortMappingProvider>();
            builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<DepartmentDto, Department>>(
                _ => DepartmentMappings.SortMapping);
            builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<AdminPatientDto, Patient>>(
                _ => AdminPatientsMappings.SortMapping);
            builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<AdminDoctorDto, Doctor>>(
                _ => AdminDoctorsMappings.SortMapping);
            builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<DoctorBasicDto, Doctor>>(
                _ => DoctorMappings.SortMapping);

            return builder;
        }

        public static WebApplicationBuilder AddEmailServices(this WebApplicationBuilder builder)
        {

            EmailConfiguration emailConfig = builder.Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>()!;

            builder.Services.AddSingleton(emailConfig);

            builder.Services.AddScoped<IEmailSender, EmailSender>();

            return builder;
        }

        public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

            JwtAuthOptions jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>()!;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Look for token in query string (Standard for SignalR)
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for the Hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                        {
                            // ...read the token from the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromHours(2));

            builder.Services.AddAuthorization();

            return builder;
        }

        public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
        {
            CorsOptions corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()!;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsOptions.PolicyName, policy =>
                {
                    policy
                        .WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddStripe(this WebApplicationBuilder builder)
        {
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            return builder;
        }

        public static WebApplicationBuilder AddFirebase(this WebApplicationBuilder builder)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
            });

            return builder;
        }

        public static WebApplicationBuilder AddHangfire(this WebApplicationBuilder builder)
        {
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("Database")));

            builder.Services.AddHangfireServer();

            return builder;
        }
    }
}
