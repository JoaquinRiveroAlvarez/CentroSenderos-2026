using CentroSenderos_2026_Servicio.ServiciosHttp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri("https://localhost:7069/")});

builder.Services.AddScoped<IHttpServicio, HttpServicio>();

await builder.Build().RunAsync();
