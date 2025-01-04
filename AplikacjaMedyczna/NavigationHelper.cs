﻿using apkaStart;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.UI.Xaml;

public static class NavigationHelper
{
    public static SplitView SplitViewInstance { get; set; }

    public static void TogglePane()
    {
        if (SplitViewInstance != null)
        {
            SplitViewInstance.IsPaneOpen = !SplitViewInstance.IsPaneOpen;
        }
    }
    public static void Navigate(string buttonName)
    {
        // Use the button name to determine the action
        switch (buttonName)
        {
            case "WpisyButton":
                NavigateToWpisy();
                break;

            case "ReceptyButton":
                NavigateToRecepty();
                break;

            case "SkierowaniaButton":
                NavigateToSkierowania();
                break;

            case "WynikiButton":
                NavigateToWyniki();
                break;

            case "BackButton":
                GoBack();
                break;

            default:
                throw new InvalidOperationException($"Unknown navigation button: {buttonName}");
        }
    }

    private static void NavigateToWpisy()
    {
        // Add navigation logic for Wpisy
        System.Diagnostics.Debug.WriteLine("Navigating to Wpisy...");
        App.MainFrame.Navigate(typeof(Wpisy), null, new DrillInNavigationTransitionInfo());
    }

    private static void NavigateToRecepty()
    {
        // Add navigation logic for Recepty
        System.Diagnostics.Debug.WriteLine("Navigating to Recepty...");
        App.MainFrame.Navigate(typeof(Recepty));
    }

    private static void NavigateToSkierowania()
    {
        // Add navigation logic for Skierowania
        System.Diagnostics.Debug.WriteLine("Navigating to Skierowania...");
        App.MainFrame.Navigate(typeof(Skierowania));
    }

    private static void NavigateToWyniki()
    {
        // Add navigation logic for Wyniki
        System.Diagnostics.Debug.WriteLine("Navigating to Wyniki...");
        App.MainFrame.Navigate(typeof(Wyniki));
    }

    private static void GoBack()
    {
        // Add logic for "Back" button
        System.Diagnostics.Debug.WriteLine("Going back...");
        if (App.MainFrame.CanGoBack)
        {
            App.MainFrame.GoBack();
        }
    }
    private static void NavbarToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        NavigationHelper.TogglePane();
    }

    private static void NavbarToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        NavigationHelper.TogglePane();
    }
}

