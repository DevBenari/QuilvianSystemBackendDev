using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Konfigurasi koneksi database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // ❗ penting
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.SerializerOptions.Converters.Add(new NullableTimeOnlyJsonConverter());
});


// Tambahkan layanan CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true) // <- ✅ allow semua origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // <- ✅ wajib untuk SignalR WebSocket
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = false;
}).AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHttpClient();

// Konfigurasi JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

//Untuk menjalankan token yang di dapat pada swagger
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    // Konversi Enum ke String
    c.MapType<PeriodeFilter>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetValues(typeof(PeriodeFilter))
            .Cast<PeriodeFilter>()
            .Select(e => new OpenApiString(e.ToString()))
            .ToList<IOpenApiAny>()
    });

    // DateTime format
    c.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time"
    });

    // Semua group
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.SwaggerDoc("manajemen_kesehatan", new OpenApiInfo { Title = "Manajemen Kesehatan API", Version = "v1" });
    c.SwaggerDoc("administrator", new OpenApiInfo { Title = "Administrator API", Version = "v1" });
    c.SwaggerDoc("hrd", new OpenApiInfo { Title = "HRD API", Version = "v1" });
    c.SwaggerDoc("master", new OpenApiInfo { Title = "Master API", Version = "v1" });

    // JWT Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan JWT dengan format: Bearer [token]"
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

});

//builder.Services.AddSwaggerGen(c =>
//{
//    c.EnableAnnotations();

//    // Konversi Enum ke String di Swagger
//    c.MapType<PeriodeFilter>(() => new OpenApiSchema
//    {
//        Type = "string",
//        Enum = Enum.GetValues(typeof(PeriodeFilter))
//            .Cast<PeriodeFilter>()
//            .Select(e => new OpenApiString(e.ToString()))
//            .ToList<IOpenApiAny>()
//    });

//    // Menampilkan Date Picker untuk startDate dan endDate
//    c.MapType<DateTime>(() => new OpenApiSchema
//    {
//        Type = "string",
//        Format = "date-time"
//    });

//    c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
//    c.SwaggerDoc("manajemen_kesehatan", new OpenApiInfo { Title = "Manajemen Kesehatan API", Version = "v1" });
//    c.SwaggerDoc("hrd", new OpenApiInfo { Title = "Administrator API", Version = "v1" });

//    // Konfigurasi JWT Authentication
//    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//        Description = "Masukkan JWT dengan format: Bearer [token]"
//    });

//    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {
//        {
//            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//            {
//                Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                {
//                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});


// add services untuk menampilkan data role
builder.Services.AddScoped<serviceMasterData>();

// add service untuk cek ttd e master ttd
builder.Services.AddScoped<ITTDService, TTDService>();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GroupArea());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Konfigurasi SignalR
// Tambahkan sebelum var app = builder.Build();
builder.Services.AddSignalR();

var app = builder.Build();

// Konfigurasi SignalR
// signal R kunjungan
app.MapHub<KunjunganHub>("/hubs/kunjungan");
app.MapHub<VitalSignHub>("/hubs/vitalsign");
app.MapHub<SOAPHub>("/hubs/soap");
app.MapHub<PainAssesmentHub>("/hubs/painassessment");

// signal R farmasi
app.MapHub<ResepHub>("/hubs/resep");
app.MapHub<ResepDetailHub>("/hubs/resepdetail");
app.MapHub<DetailPenerimaanHub>("/hubs/detailpenerimaan");
app.MapHub<DetailPermintaanHub>("/hubs/detailpermintaan");
app.MapHub<PenerimaanUnitHub>("/hubs/penerimaanunit");
app.MapHub<PermintaanUnitHub>("/hubs/permintaanunit");

// signal R Ranap
app.MapHub<SuratPengantarRanapHub>("/hubs/suratpengantarranap");
app.MapHub<AssessmentEdukasiDetailHub>("/hubs/assessmentedukasidetail");
app.MapHub<AssessmentEdukasiHub>("/hubs/assessmentedukasi");

// signal R IGD
app.MapHub<IGDTriageHub>("/hubs/IGDtriage");
app.MapHub<PindahRuanganHub>("/hubs/pindahruangan");
app.MapHub<IGDAssessmentAwalHub>("/hubs/IGDassessmentawal");
app.MapHub<NosokomialHub>("/hubs/nosokomial");

// signal R Laboratorium
app.MapHub<LabBookingHub>("/hubs/labbooking");
app.MapHub<LabBookingDetailHub>("/hubs/labbookingdetail");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home");
        c.SwaggerEndpoint("/swagger/manajemen_kesehatan/swagger.json", "Manajemen Kesehatan API");
        c.SwaggerEndpoint("/swagger/administrator/swagger.json", "Administrator API");
        c.SwaggerEndpoint("/swagger/hrd/swagger.json", "HRD API");
        c.SwaggerEndpoint("/swagger/master/swagger.json", "Master API");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home");
    c.SwaggerEndpoint("/swagger/manajemen_kesehatan/swagger.json", "Manajemen Kesehatan API");
    c.SwaggerEndpoint("/swagger/administrator/swagger.json", "Administrator API");
    c.SwaggerEndpoint("/swagger/hrd/swagger.json", "HRD API");
    c.SwaggerEndpoint("/swagger/master/swagger.json", "Master API");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowSpecific"); // Panggil sebelum middleware lainnya
app.UseAuthentication(); // Tambahkan middleware autentikasi
app.UseAuthorization();
app.MapControllers();

app.Run();