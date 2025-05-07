using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDynamics.Shared;
using AutoDynamics.Shared.Layout;
using AutoDynamics.Shared.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
namespace AutoDynamics.Services
{
    public class MultiWindowService : IMultiWindowService
    {
        public void OpenBlazorPageInNewWindow(string route, string title)
        {
            var blazorHost = new BlazorWebView
            {
                HostPage = "wwwroot/index.html",       // your standard Blazor host page
                StartPath = route,                      // e.g. "/credit/page"
                
                RootComponents =
            {
                // we mount the root App.razor here so the Router kicks in!
                new RootComponent
                {
                    Selector      = "#app",
                    ComponentType = typeof(Routes)       // your Blazor App component
                }
            }
            };

            var win = new Window(new ContentPage
            {
                Title = title,
                Content = blazorHost
            });

            Application.Current.OpenWindow(win);
        }
    }
}
