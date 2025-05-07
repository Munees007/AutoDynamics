using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AutoDynamics.Shared.Modals
{
    public class TabItem
    {
        public string Title { get; set; }
        public string Path { get; set; } // Optional
        public Type ComponentType { get; set; }
        public RenderFragment Content;
        public bool IsClosable { get; set; } = true;
    }

}
