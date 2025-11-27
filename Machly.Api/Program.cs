using Machly.Api.Config;
using Machly.Api.Repositories;
using Machly.Api.Seed;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// 1. CONFIGURACI�N MONGODB
// ------------------------------------------------------
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbContext>();

// ------------------------------------------------------
// 2. SINGLETONS / HELPERS
// ------------------------------------------------------
builder.Services.AddSingleton<JwtHelper>();

// ------------------------------------------------------
// 3. SERVICIOS Y REPOSITORIOS (SCOPED)
// ------------------------------------------------------
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationSender, MockNotificationSender>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MachineRepository>();
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<FavoriteRepository>();
builder.Services.AddScoped<SupportTicketRepository>();
builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<AuditRepository>();

builder.Services.AddScoped<AuditService>();

// ------------------------------------------------------
// 4. CONTROLLERS & SIGNALR
// ------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ------------------------------------------------------
// 5. SWAGGER
// ------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Machly API",
        Version = "v1"
    });

    // Agregar seguridad JWT a Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------------------------------------------
// 6. CORS
// ------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithOrigins("http://localhost:5000", "https://localhost:5001") // Ajustar según cliente
              .AllowCredentials()); // Necesario para SignalR
});

// ------------------------------------------------------
// 7. JWT AUTH
// ------------------------------------------------------
var jwtKey = builder.Configuration["JwtSettings:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    // RequireHttpsMetadata se refiere a los metadatos del issuer, no a la conexión HTTPS
    // Para desarrollo local, puede ser false. En producción debería ser true si se usa un issuer HTTPS
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };

    // Configuración para SignalR (token en query string)
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ------------------------------------------------------
// 8. GRAPHQL
// ------------------------------------------------------
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Machly.Api.GraphQL.Queries.Query>()
    .AddMutationType<Machly.Api.GraphQL.Mutations.Mutation>()
    .AddType<Machly.Api.GraphQL.Types.MachineType>()
    .AddType<Machly.Api.GraphQL.Types.BookingType>()
    .AddType<Machly.Api.GraphQL.Types.UserType>()
    .AddAuthorization()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

// ======================================================
//                BUILD APP
// ======================================================

var app = builder.Build();

app.UseCors("AllowAll");

// Swagger solo para development (pero t lo quieres siempre visible)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Machly API v1");
    c.RoutePrefix = string.Empty; // swagger en "/"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");
app.MapHub<Machly.Api.Hubs.ChatHub>("/chatHub");


// ======================================================
//                SEED AUTOM�TICO (DEV)
// ======================================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var userRepo = services.GetRequiredService<UserRepository>();
    var machineRepo = services.GetRequiredService<MachineRepository>();
    var bookingRepo = services.GetRequiredService<BookingRepository>();

    await SeedData.InitializeAsync(userRepo, machineRepo, bookingRepo);
}

app.Run();
