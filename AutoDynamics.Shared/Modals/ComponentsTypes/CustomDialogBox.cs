using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.ComponentsTypes
{
    public class CustomDialogBox
    {
        public string Title { get; set; } = "";
        public bool showDialog { set; get; } = false;
        public RenderFragment body { get; set; }
        public List<FooterBtn> footers { get; set; } = new();
        public EventCallback OnCloase { get; set; }

        public string AnimationClass => showDialog ? "animate-fade-in animate-slide-in" : "animate-fade-out animate-slide-out";

    }

    public class FooterBtn
    {
        public RenderFragment Button { get; set; }
        public EventCallback EventCallback;
    }
}
