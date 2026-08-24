using CentroSenderos_2026_BD;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Components;
using CentroSenderos_2026_Server.Components.Account;
using CentroSenderos_2026_Servicio.ServiciosHttp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configuración de HttpClient para el cliente Blazor
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl no está configurado.");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Servicios generales
builder.Services.AddScoped<IHttpServicio, HttpServicio>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CentroSenderos-2026 API",
        Version = "v1",
        Description = "API de gestión",
    });
});

// Configuración de base de datos
var strConn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión no existe.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(strConn));

// Repositorios
builder.Services.AddScoped<IProfesionalRepositorio, ProfesionalRepositorio>();
builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<ITipoObraSocialRepositorio, TipoObraSocialRepositorio>();
builder.Services.AddScoped<ITipoTurnoRepositorio, TipoTurnoRepositorio>();
builder.Services.AddScoped<ITipoPrestacionRepositorio, TipoPrestacionRepositorio>();
builder.Services.AddScoped<ITipoDiagnosticoRepositorio, TipoDiagnosticoRepositorio>();
builder.Services.AddScoped<ISocioRepositorio, SocioRepositorio>();
builder.Services.AddScoped<ITipoPlanillaRepositorio, TipoPlanillaRepositorio>();
builder.Services.AddScoped<ITurnoRepositorio, TurnoRepositorio>();
builder.Services.AddScoped<ITipoConsultorioRepositorio, TipoConsultorioRepositorio>();

// Razor Components + Auth
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Configuración de Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<MiUsuario>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdmin", policy => policy.RequireRole("admin"));
    options.AddPolicy("EsEquipo", policy => policy.RequireRole("equipo", "admin"));
    options.AddPolicy("EsProfesional", policy => policy.RequireRole("profesional", "equipo", "admin"));
});

builder.Services.AddSingleton<IEmailSender<MiUsuario>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CentroSenderos-2026 API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CentroSenderos_2026_Server.Client._Imports).Assembly);

// Controllers (incluye SeguridadController)
app.MapControllers();
app.MapAdditionalIdentityEndpoints();

app.Run();
