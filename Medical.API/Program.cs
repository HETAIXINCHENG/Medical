using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Medical.API.Data;
using Medical.API.Repositories;
using Medical.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 配置 JSON 序列化选项，处理循环引用
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // System.Text.Json 默认会包含 null 值，不需要额外配置
    });

// 配置数据库
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MedicalDbContext>(options =>
    options.UseNpgsql(connectionString));

// 注册服务
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 注册 HttpClient（用于调用外部API）
builder.Services.AddHttpClient();

// 配置 JWT 认证
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "MedicalSystemSecretKey2024!@#$%^&*()VeryLongKeyForSecurity";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MedicalAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MedicalClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // 配置事件处理，允许匿名访问
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 如果没有 token，检查端点是否允许匿名访问
            if (string.IsNullOrEmpty(context.Token))
            {
                var endpoint = context.HttpContext.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
                {
                    // 允许匿名访问，跳过认证
                    context.NoResult();
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // 如果端点允许匿名访问，则跳过认证失败处理
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            {
                context.NoResult();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // 如果端点允许匿名访问，则跳过挑战（不返回 401）
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            {
                context.HandleResponse();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    };
});

// 配置授权
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor", "Admin", "SuperAdmin"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient", "Admin", "SuperAdmin"));
    
    // 默认策略：允许匿名访问（这样静态文件可以正常访问）
    options.FallbackPolicy = null; // 不设置默认策略，允许匿名访问
});

// 配置 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "医疗咨询系统 API",
        Version = "v1",
        Description = "医疗咨询系统 RESTful API 文档",
        Contact = new OpenApiContact
        {
            Name = "医疗系统开发团队",
            Email = "support@medical.com"
        }
    });

    // 添加 JWT 认证到 Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT 授权头使用 Bearer 方案。例如: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 包含 XML 注释
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "医疗咨询系统 API v1");
        c.RoutePrefix = string.Empty; // 设置 Swagger UI 为根路径
    });
}

// 只在生产环境或配置了 HTTPS 时启用 HTTPS 重定向
// 检查应用程序 URL 是否包含 HTTPS
var applicationUrl = builder.Configuration["applicationUrl"] ?? 
                     Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var hasHttps = !string.IsNullOrEmpty(applicationUrl) && 
               applicationUrl.Contains("https://", StringComparison.OrdinalIgnoreCase);

// 只在生产环境或实际配置了 HTTPS 时启用 HTTPS 重定向
// 这样可以避免在开发环境中只使用 HTTP 时出现 "Failed to determine the https port for redirect" 警告
if (app.Environment.IsProduction() || hasHttps)
{
    app.UseHttpsRedirection();
}

// 配置静态文件服务（用于访问上传的图片）
// 注意：静态文件服务必须在认证和授权之前，以便匿名访问
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// 配置静态文件服务，允许匿名访问
var staticFileOptions = new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // 设置 CORS 头，允许跨域访问
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        
        // 确保 webp 文件返回正确的 Content-Type
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".webp"))
        {
            ctx.Context.Response.ContentType = "image/webp";
        }
        else if (path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
        {
            ctx.Context.Response.ContentType = "image/jpeg";
        }
        else if (path.EndsWith(".png"))
        {
            ctx.Context.Response.ContentType = "image/png";
        }
        else if (path.EndsWith(".gif"))
        {
            ctx.Context.Response.ContentType = "image/gif";
        }
    }
};

app.UseStaticFiles(staticFileOptions);

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// 确保数据库已创建并初始化数据
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MedicalDbContext>();
    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // 应用数据库迁移（Migration方式）
        // 如果数据库已存在但迁移历史表不存在，需要先创建迁移历史表并标记迁移为已应用
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            try
            {
                // 检查迁移历史表是否存在
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory';
                ";
                var result = await command.ExecuteScalarAsync();
                var tableExists = Convert.ToInt32(result) > 0;
                await connection.CloseAsync();
                
                // 如果迁移历史表不存在，创建它并插入已存在的迁移记录
                if (!tableExists)
                {
                    // 创建迁移历史表
                    await dbContext.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                            ""MigrationId"" VARCHAR(150) NOT NULL,
                            ""ProductVersion"" VARCHAR(32) NOT NULL,
                            CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                        );
                    ");
                    
                    // 插入已存在的迁移记录（根据实际迁移文件名）
                    // 注意：InitialCreate 是空的，所以只需要插入 UserDoctorSubscription
                    await dbContext.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                        VALUES 
                            ('20251208052213_InitialCreate', '9.0.10'),
                            ('20251208051722_UserDoctorSubscription', '9.0.10')
                        ON CONFLICT (""MigrationId"") DO NOTHING;
                    ");
                }
            }
            catch (Exception ex)
            {
                // 如果检查失败，记录日志但继续执行迁移
                logger.LogWarning(ex, "检查迁移历史表时出错，将继续执行迁移: {Error}", ex.Message);
            }
        }
        // 检查数据库是否为空（没有任何表）
        bool databaseIsEmpty = false;
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE'
                AND table_name != '__EFMigrationsHistory';
            ";
            var result = await command.ExecuteScalarAsync();
            var tableCount = Convert.ToInt32(result);
            await connection.CloseAsync();
            databaseIsEmpty = tableCount == 0;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "检查数据库表数量时出错，将尝试使用迁移方式: {Error}", ex.Message);
        }
        
        // 如果数据库为空，使用 EnsureCreated 自动创建所有表
        // 否则使用 Migrate 应用迁移
        if (databaseIsEmpty)
        {
            logger.LogInformation("检测到空数据库，使用 EnsureCreated 自动创建所有表...");
            dbContext.Database.EnsureCreated();
            logger.LogInformation("数据库表创建完成");
        }
        else
        {
            // 尝试应用迁移
            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "应用迁移失败，尝试使用 EnsureCreated: {Error}", ex.Message);
                // 如果迁移失败，回退到 EnsureCreated
                dbContext.Database.EnsureCreated();
            }
        }

    }
    catch (Exception ex)
    {
        var programLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        programLogger.LogError(ex, "数据库初始化失败");
    }
}

app.Run();

