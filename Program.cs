using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Hubs;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Controllers;
using QuilvianSystemBackendDev.Hangfire.Controllers;
using QuilvianSystemBackendDev.Hangfire.Jobs;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

var builder = WebApplication.CreateBuilder(args);

#region Setting Database
// Add services to the container.
// Konfigurasi koneksi database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region MVC JSON
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

//builder.Services.Configure<NilaiPPN>(
//    builder.Configuration.GetSection("PPN")
//    );


#endregion

#region Hangfire
// BUILDER HANGFIRE
//builder.Services.AddControllers();

//// 1) Register Hangfire + Storage
//builder.Services.AddHangfire(config => config
//    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
//    .UseSimpleAssemblyNameTypeSerializer()
//    .UseRecommendedSerializerSettings()
//    .UsePostgreSqlStorage(opts =>
//        opts.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
//);

//static TimeZoneInfo GetJakartaTimeZone()
//{
//    try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }          // Linux
//    catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } // Windows
//}

//var tz = GetJakartaTimeZone();

//// 2) Jalankan Hangfire Server (worker) di proses web ini
//builder.Services.AddHangfireServer();


//RecurringJob.AddOrUpdate<BillingJob>(
//    "update-dpd-billing",
//    job => job.DPDBillingRunAsync(CancellationToken.None),
//    "5 0 * * *", // 00:05 setiap hari
//    new RecurringJobOptions { TimeZone = tz }
//);
#endregion

#region CORS
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
#endregion

#region JWT
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
    c.SwaggerDoc("finance", new OpenApiInfo { Title = "Finance API", Version = "v1" });
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

#endregion

#region Areas Service Umum
// add services untuk menampilkan data role
builder.Services.AddScoped<serviceMasterData>();
// add service untuk cek ttd e master ttd
builder.Services.AddScoped<ITTDService, TTDService>();
// add service untuk update status billing 
builder.Services.AddScoped<IBillingService, BillingPaidService>();
// add service untuk generate no rm unique
builder.Services.AddScoped<INoRMGeneratorService, NoRMGeneratorService>();
// service generate no kwitansi unique
builder.Services.AddScoped<INoKwitansiService, NoKwitansiService>();
// add service generate no angsuran
builder.Services.AddScoped<IGenerateUrutanAngsuran, GenerateUrutanAngsuranService>();
// service hitung jumlah angsuran
builder.Services.AddScoped<ICountAngsuran, CountAngsuranService>();
// service generate invoice di billing
builder.Services.AddScoped<IGenerateInvoiceBillingService, GenerateInvoiceBillingService>();
//service get data billing per kunjungan
builder.Services.AddScoped<IBillingKunjunganReadService, BillingKunjunganReadService>();
// service get prakiraan billing kunjungan IP
builder.Services.AddScoped<IPerkiraanBillingRanapService, PerkiraanBillingRanapService>();
// service kwitansi deposit ranap
builder.Services.AddScoped<IDepositRanapNumberService, DepositRanapNumberService>();
// service asuransi coverage
builder.Services.AddScoped<IAsuransiCoverageService, AsuransiCoverageService>();
#endregion

#region Setting Container
// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GroupArea());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
#endregion

#region Setting SignalR
// Konfigurasi SignalR
builder.Services.AddSignalR();

var app = builder.Build();
app.UseRouting();
app.UseCors("AllowSpecific"); // Panggil sebelum middleware lainnya


// Konfigurasi SignalR
// signal R kunjungan
app.MapControllers();
#region Hubs SignalR
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
app.MapHub<MonitoringNyeriHub>("/hubs/monitoringnyeri");

// signal R IGD
app.MapHub<IGDTriageHub>("/hubs/IGDtriage");
app.MapHub<PindahRuanganHub>("/hubs/pindahruangan");
app.MapHub<IGDAssessmentAwalHub>("/hubs/IGDassessmentawal");
app.MapHub<NosokomialHub>("/hubs/nosokomial");
app.MapHub<HandoverPasienHub>("/hubs/handoverpasien");

// signal R Laboratorium
app.MapHub<LabBookingHub>("/hubs/labbooking");
app.MapHub<LabBookingDetailHub>("/hubs/labbookingdetail");

// signal R Alkes
app.MapHub<AlatPemakaianHub>("/hubs/alatpemakaian");

// signal R Diskon
app.MapHub<DiskonHub>("/hubs/diskon");
app.MapHub<DiskonApprovedHub>("/hubs/diskonapproved");
app.MapHub<DiskonDokterHub>("/hubs/diskondokter");
#endregion

#endregion

#region Setting HTTP dan Swagger
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
        c.SwaggerEndpoint("/swagger/finance/swagger.json", "Finance API");
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
    c.SwaggerEndpoint("/swagger/finance/swagger.json", "Finance API");
    c.SwaggerEndpoint("/swagger/master/swagger.json", "Master API");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

#endregion

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication(); // Tambahkan middleware autentikasi
app.UseAuthorization();

//app.MapHangfireDashboard("/hangfire", new DashboardOptions
//{
//    Authorization = new[] { new HangfireDashboardAuthFilterController() }
//});
app.Run();