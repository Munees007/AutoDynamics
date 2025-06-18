// TabService.cs
using AutoDynamics.Shared.Pages;
using AutoDynamics.Shared.Services;
using AutoDynamics.Shared.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class TabService : ITabService
{
    public List<TabItem> Tabs { get; private set; } = new();
    public TabItem? Selected { get; private set; }

    public event Action? OnChange;

    
    public void OpenTab(string title, Type componentType, bool isClosable = true,bool ignoreDuplicate = false)
    {
        try
        {
            var existing = Tabs.FirstOrDefault(t => t.Title == title && t.ComponentType == componentType);
            if (existing != null && ignoreDuplicate == false)
            {
                Selected = existing;
            }
            else
            {
                var newTab = new TabItem
                {
                    Title = title,
                    Content = builder =>
                    {
                        builder.OpenComponent(0, componentType);
                        builder.CloseComponent();
                    },
                    ComponentType = componentType,
                    IsClosable = isClosable
                };
                Tabs.Add(newTab);
                Selected = newTab;
            }

            NotifyStateChanged();
        }
        catch(Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        
    }

    public void OpenWithParameter(string title, Type componentType, Dictionary<string,dynamic> parameters, bool isClosable = true)
    {
        var existing = Tabs.FirstOrDefault(t => t.Title == title && t.ComponentType == componentType);
        if (existing != null)
        {
            Selected = existing;
        }
        else
        {
            var newTab = new TabItem
            {
                Title = title,
                Content = builder =>
                {
                    builder.OpenComponent(0, componentType);
                    int attributeNum = 1;
                    foreach(var parameter in parameters)
                    {
                        builder.AddAttribute(attributeNum, parameter.Key, parameter.Value);
                        attributeNum++;
                    }
                    builder.CloseComponent();
                },
                ComponentType = componentType,
                IsClosable = isClosable
            };
            Tabs.Add(newTab);
            Selected = newTab;
        }

        NotifyStateChanged();
    }

    public void OpenTabByPath(string title, string path, Type componentType)
    {
        var existing = Tabs.FirstOrDefault(t => t.Path == path);
        if (existing is not null)
        {
            Selected = existing;
        }
        else
        {
            var tab = new TabItem
            {
                Title = title,
                Path = path,
                ComponentType = componentType,
                IsClosable = true
            };
            Tabs.Add(tab);
            Selected = tab;
        }

        NotifyStateChanged();
    }

    public void CloseTab(TabItem tab)
    {
        if (!Tabs.Contains(tab)) return;

        Tabs.Remove(tab);
        if (Selected == tab)
        {
            Selected = Tabs.LastOrDefault();
        }

        NotifyStateChanged();
    }

    public void SelectTab(TabItem tab)
    {
        if (Tabs.Contains(tab))
        {
            Selected = tab;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
