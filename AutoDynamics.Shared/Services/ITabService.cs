using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDynamics.Shared.Modals;

namespace AutoDynamics.Shared.Services
{
    public interface ITabService
    {

        public List<TabItem> Tabs { get; }
        public TabItem? Selected { get; }

        event Action? OnChange;

        void OpenTab(string title, Type componentType, bool isClosable = true,bool ignoreDuplicate = false);
        void OpenTabByPath(string title, string path, Type componentType);

        void OpenWithParameter(string title, Type componentType, Dictionary<string, dynamic> parameters, bool isClosable = true);
        void CloseTab(TabItem tab);
        void SelectTab(TabItem tab);
    }
}
