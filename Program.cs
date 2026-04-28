using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
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
    //var idleSeconds = builder.Configuration.GetValue<int?>("AuthSession:IdleTimeoutSeconds");

    //if (idleSeconds.HasValue && idleSeconds.Value > 0)
    //{
    //    options.IdleTimeout = TimeSpan.FromSeconds(idleSeconds.Value);
    //}
    //else
    //{
    //    var idleMinutes = builder.Configuration.GetValue<int?>("AuthSession:IdleTimeoutMinutes") ?? 180;
    //    options.IdleTimeout = TimeSpan.FromMinutes(idleMinutes);
    //}
    var idleMinutes = builder.Configuration.GetValue<int?>("AuthSession:IdleTimeoutMinutes") ?? 180;

    //options.IdleTimeout = TimeSpan.FromMinutes(idleMinutes);
    options.Cookie.Name = ".Quilvian.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    //kalo pake https ini dinyalakan
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

#endregion


#region AUTH JWT + COOKIE SMART SCHEME
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartAuth";
    options.DefaultAuthenticateScheme = "SmartAuth";
    options.DefaultChallengeScheme = "SmartAuth";
})
.AddPolicyScheme("SmartAuth", "JWT Bearer atau Identity Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authorization = context.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorization) &&
            authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        return IdentityConstants.ApplicationScheme;
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
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
                message = message
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
});
#endregion

#region COOKIE IDENTITY

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".Quilvian.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    //kalo pake https ini dinyalakan
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int?>("AuthSession:CookieExpireMinutes") ?? 180
    );

    //var cookieSeconds = builder.Configuration.GetValue<int?>("AuthSession:CookieExpireSeconds");

    //if (cookieSeconds.HasValue && cookieSeconds.Value > 0)
    //{
    //    options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSeconds.Value);
    //}
    //else
    //{
    //    var cookieMinutes = builder.Configuration.GetValue<int?>("AuthSession:CookieExpireMinutes") ?? 180;
    //    options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMinutes);
    //}

    options.SlidingExpiration = true;
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/forbidden";

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = "error",
                code = 401,
                message = "Unauthorized. Token tidak ditemukan atau session login sudah berakhir."
            }));
        },

        OnRedirectToAccessDenied = async context =>
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

});

#endregion

#region AUTHORIZATION

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder("SmartAuth")
        .RequireAuthenticatedUser()
        .Build();
});

#endregion

#region SWAGGER
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

    // Semua group swagger
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quilvian API", Version = "v1" });
    c.SwaggerDoc("manajemen_kesehatan", new OpenApiInfo { Title = "Manajemen Kesehatan API", Version = "v1" });
    c.SwaggerDoc("administrator", new OpenApiInfo { Title = "Administrator API", Version = "v1" });
    c.SwaggerDoc("hrd", new OpenApiInfo { Title = "HRD API", Version = "v1" });
    c.SwaggerDoc("finance", new OpenApiInfo { Title = "Finance API", Version = "v1" });
    c.SwaggerDoc("master", new OpenApiInfo { Title = "Master API", Version = "v1" });

    // Penting: cocokkan docName dengan ApiExplorer.GroupName
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var groupName = apiDesc.GroupName;

        // kalau controller tidak punya group, masukkan ke v1
        if (string.IsNullOrWhiteSpace(groupName))
            return docName == "v1";

        return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
    });

    // JWT Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan JWT dengan format: {token}, tanpa bearer"
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
builder.Services.AddScoped<INotification, NotificationService>();
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

app.UseHttpsRedirection();

app.UseStaticFiles();

/*
 * Swagger dibuat public.
 * Jadi user belum login tetap bisa membuka halaman Swagger.
 */
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
    // Supaya token tetap tersimpan saat pindah-pindah definition / reload Swagger
    c.EnablePersistAuthorization();
});

#endregion

app.UseRouting();

app.UseCors("AllowSpecific");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

#region Hubs + Controllers

app.MapControllers();

app.MapHub<KunjunganHub>("/hubs/kunjungan");
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