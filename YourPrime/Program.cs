using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YourPrime.Auth;
using YourPrime.Data;
using YourPrime.Interfaces;
using YourPrime.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------- CONTROLLERS -------------------
builder.Services.AddControllers();

// ------------------- SWAGGER -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите JWT токен: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// ------------------- DB CONTEXT -------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------- JWT -------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

if (jwtSettings is null)
    throw new Exception("JWT settings are missing in configuration");

// ------------------- AUTH -------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),

            ClockSkew = TimeSpan.Zero
        };
    });

// ------------------- AZURE BLOB -------------------
builder.Services.Configure<AzureBlobSettings>(
    builder.Configuration.GetSection("AzureBlobStorage"));

// ------------------- AUTHORIZATION -------------------
builder.Services.AddAuthorization();

// ------------------- CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000") // <-- Docker frontend port
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ------------------- DI SERVICES -------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITournamentService, TournamentService>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IBlobService, BlobService>();

var app = builder.Build();

// ------------------- AUTO MIGRATION (IMPORTANT) -------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ------------------- PIPELINE ORDER -------------------

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// IMPORTANT: HTTPS (можно оставить, но в Docker иногда мешает)
app.UseHttpsRedirection();

// CORS
app.UseCors("ReactPolicy");

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();