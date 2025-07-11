using AutoDynamics.Web.Components;
using AutoDynamics.Shared.Services;
using AutoDynamics.Web.Services;
using Blazored.LocalStorage;
using AutoDynamics.Web.Helper;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

//Add Local Storage
builder.Services.AddBlazoredLocalStorage();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient());

// Add device-specific services used by the AutoDynamics.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IFileHelper, FileHelper>();
builder.Services.AddSingleton<IDatabaseHandler, DatabaseHandler>();
builder.Services.AddSingleton<ICurrentData,CurrentData>();
builder.Services.AddSingleton<IPDFGenerator,PDFGenerator>();
builder.Services.AddSingleton<IWhatsAppService,WhatsAppService>();
builder.Services.AddScoped<IMyLocalStorageService,MyLocalStorageServices>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<JSHelper>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AutoDynamics.Shared._Imports).Assembly);

app.Run();
