using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Hubs;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services;
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region MVC JSON

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.SerializerOptions.Converters.Add(new NullableTimeOnlyJsonConverter());
});

#endregion

builder.Services.Configure<AutoLoginDTO>(builder.Configuration.GetSection("AutoLogin"));

#region CORS

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

#endregion

#region IDENTITY

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
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHttpClient();

#endregion

#region SESSION

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    var idleMinutes = builder.Configuration.GetValue<int?>("AuthSession:IdleTimeoutMinutes") ?? 180;
    var idleTimeout = TimeSpan.FromMinutes(idleMinutes);

    options.IdleTimeout = idleTimeout;

    options.Cookie.Name = ".Quilvian.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    
    
    // Opsional: supaya Chrome menampilkan Max-Age/Expires,
    // tapi timeout server tetap dikontrol oleh IdleTimeout.
    options.Cookie.MaxAge = idleTimeout;
});

#endregion

#region AUTH JWT COOKIE

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtCookieName = builder.Configuration["Jwt:CookieName"] ?? ".Quilvian.AccessToken";

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key belum dikonfigurasi di appsettings.json.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey)
);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var message = "Unauthorized. Token tidak ditemukan, tidak valid, atau sudah expired.";

                if (context.AuthenticateFailure is SecurityTokenExpiredException)
                {
                    message = "Unauthorized. Token sudah expired. Silakan login ulang.";
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = "error",
                    code = 401,
                    message
                }));
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = "error",
                    code = 403,
                    message = "Forbidden. Anda tidak memiliki akses ke endpoint ini."
                }));
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHttpContextAccessor();

#endregion

#region AUTHORIZATION

builder.Services.AddAuthorization();

#endregion

#region SWAGGER

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.MapType<PeriodeFilter>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetValues(typeof(PeriodeFilter))
            .Cast<PeriodeFilter>()
            .Select(e => new OpenApiString(e.ToString()))
            .ToList<IOpenApiAny>()
    });

    c.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time"
    });

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quilvian API", Version = "v1" });
    c.SwaggerDoc("manajemen_kesehatan", new OpenApiInfo { Title = "Manajemen Kesehatan API", Version = "v1" });
    c.SwaggerDoc("administrator", new OpenApiInfo { Title = "Administrator API", Version = "v1" });
    c.SwaggerDoc("hrd", new OpenApiInfo { Title = "HRD API", Version = "v1" });
    c.SwaggerDoc("finance", new OpenApiInfo { Title = "Finance API", Version = "v1" });
    c.SwaggerDoc("master", new OpenApiInfo { Title = "Master API", Version = "v1" });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var groupName = apiDesc.GroupName;

        if (string.IsNullOrWhiteSpace(groupName))
            return docName == "v1";

        return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT saja, tanpa kata Bearer."
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

builder.Services.AddScoped<serviceMasterData>();
builder.Services.AddScoped<ITTDService, TTDService>();
builder.Services.AddScoped<IBillingService, BillingPaidService>();
builder.Services.AddScoped<INoRMGeneratorService, NoRMGeneratorService>();
builder.Services.AddScoped<INoKwitansiService, NoKwitansiService>();
builder.Services.AddScoped<IGenerateUrutanAngsuran, GenerateUrutanAngsuranService>();
builder.Services.AddScoped<ICountAngsuran, CountAngsuranService>();
builder.Services.AddScoped<IGenerateInvoiceBillingService, GenerateInvoiceBillingService>();
builder.Services.AddScoped<IBillingKunjunganReadService, BillingKunjunganReadService>();
builder.Services.AddScoped<IPerkiraanBillingRanapService, PerkiraanBillingRanapService>();
builder.Services.AddScoped<IDepositRanapNumberService, DepositRanapNumberService>();
builder.Services.AddScoped<IAsuransiCoverageService, AsuransiCoverageService>();
builder.Services.AddHttpClient<INotification, NotificationService>(
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddScoped<IKunjunganAdminBillingService, KunjunganAdminBillingService>();
builder.Services.AddScoped<IKunjunganNoRegistrasiService, KunjunganNoRegistrasiService>();
builder.Services.AddScoped<INoBillService, NoBillService>();
builder.Services.AddScoped<IObatUnitStockService, ObatUnitReserveService>();
builder.Services.AddScoped<IResepStockService, ResepStockService>();
builder.Services.AddScoped<INoPhotoGeneratorService, NoPhotoGeneratorService>();
builder.Services.AddScoped<ILabBillingService, LabBillingService>();

#endregion

#region Setting Container

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GroupArea());
});

builder.Services.AddEndpointsApiExplorer();

#endregion

#region Setting SignalR

builder.Services.AddSignalR();

#endregion

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

#region Swagger

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home");
    c.SwaggerEndpoint("/swagger/manajemen_kesehatan/swagger.json", "Manajemen Kesehatan API");
    c.SwaggerEndpoint("/swagger/administrator/swagger.json", "Administrator API");
    c.SwaggerEndpoint("/swagger/hrd/swagger.json", "HRD API");
    c.SwaggerEndpoint("/swagger/finance/swagger.json", "Finance API");
    c.SwaggerEndpoint("/swagger/master/swagger.json", "Master API");

    c.RoutePrefix = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

#endregion

app.UseRouting();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        var origin = context.Request.Headers["Origin"].ToString();

        if (!string.IsNullOrWhiteSpace(origin) &&
            allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Vary"] = "Origin";

            var requestMethod = context.Request.Headers["Access-Control-Request-Method"].ToString();
            context.Response.Headers["Access-Control-Allow-Methods"] =
                string.IsNullOrWhiteSpace(requestMethod)
                    ? "GET,POST,PUT,PATCH,DELETE,OPTIONS"
                    : requestMethod;

            var requestHeaders = context.Request.Headers["Access-Control-Request-Headers"].ToString();
            context.Response.Headers["Access-Control-Allow-Headers"] =
                string.IsNullOrWhiteSpace(requestHeaders)
                    ? "authorization,content-type"
                    : requestHeaders;
        }

        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});
app.UseCors("FrontendCorsPolicy");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

#region Hubs + Controllers

app.MapControllers();
   
app.MapHub<KunjunganHub>("/hubs/kunjungan");
app.MapHub<TindakanKunjunganHub>("/hubs/tindakankunjungan");
app.MapHub<VitalSignHub>("/hubs/vitalsign");
app.MapHub<SOAPHub>("/hubs/soap");
app.MapHub<PainAssesmentHub>("/hubs/painassessment");

app.MapHub<ResepHub>("/hubs/resep");
app.MapHub<ResepDetailHub>("/hubs/resepdetail");
app.MapHub<DetailPenerimaanHub>("/hubs/detailpenerimaan");
app.MapHub<DetailPermintaanHub>("/hubs/detailpermintaan");
app.MapHub<PenerimaanUnitHub>("/hubs/penerimaanunit");
app.MapHub<PermintaanUnitHub>("/hubs/permintaanunit");

app.MapHub<SuratPengantarRanapHub>("/hubs/suratpengantarranap");
app.MapHub<AssessmentEdukasiDetailHub>("/hubs/assessmentedukasidetail");
app.MapHub<AssessmentEdukasiHub>("/hubs/assessmentedukasi");
app.MapHub<MonitoringNyeriHub>("/hubs/monitoringnyeri");

app.MapHub<IGDTriageHub>("/hubs/IGDtriage");
app.MapHub<PindahRuanganHub>("/hubs/pindahruangan");
app.MapHub<IGDAssessmentAwalHub>("/hubs/IGDassessmentawal");
app.MapHub<NosokomialHub>("/hubs/nosokomial");
app.MapHub<HandoverPasienHub>("/hubs/handoverpasien");

app.MapHub<LabBookingHub>("/hubs/labbooking");
app.MapHub<LabBookingDetailHub>("/hubs/labbookingdetail");

app.MapHub<AlatPemakaianHub>("/hubs/alatpemakaian");

app.MapHub<DiskonHub>("/hubs/diskon");
app.MapHub<DiskonApprovedHub>("/hubs/diskonapproved");
app.MapHub<DiskonDokterHub>("/hubs/diskondokter");

#endregion

app.Run();