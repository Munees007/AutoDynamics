using Microsoft.Extensions.Logging;
using AutoDynamics.Shared.Services;
using AutoDynamics.Services;

namespace AutoDynamics;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        

        // Add device-specific services used by the AutoDynamics.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddSingleton<IFileHelper, FileHelper>();
        builder.Services.AddSingleton<IDatabaseHandler, DatabaseHandler>();
        builder.Services.AddSingleton<ICurrentData,CurrentData>();
        builder.Services.AddSingleton<IPDFGenerator,PDFGenerator>();
        builder.Services.AddSingleton<IWhatsAppService,WhatsAppService>();
        builder.Services.AddSingleton<IDownloadExcel,DownloadExcel>();
        builder.Services.AddSingleton<IToastService,ToastService>();
        builder.Services.AddSingleton<IMultiWindowService, MultiWindowService>();
        builder.Services.AddSingleton<ITabService,TabService>();
        builder.Services.AddScoped<IMyLocalStorageService, MyLocalStorageService>();
        builder.Services.AddScoped<IAlertService, AlertService>();
        
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    
}
