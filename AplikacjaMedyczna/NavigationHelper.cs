using AplikacjaMedyczna;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

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
        switch (buttonName)
        {
            case "Logout":
                Logout();
                break;
            case "PatientChoiceButton":
                NavigateToPatientChoice();
                break;
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
            case "ManageWorkersButton":
                NavigateToManageWorkers();
                break;
            case "InsertPatientButton":
                NavigateToManagePatients();
                break;
            case "PanelButton":
                NavigateToPanelGlowny();
                break;
            default:
                throw new InvalidOperationException($"Unknown navigation button: {buttonName}");
        }
    }

    private static void NavigateToManageWorkers()
    {
        // Add navigation logic for ManageWorkers
        System.Diagnostics.Debug.WriteLine("Navigating to ManageWorkers...");
        App.MainFrame.Navigate(typeof(Admin_Panel), null, new DrillInNavigationTransitionInfo());
    }
    private static void NavigateToManagePatients()
    {
        // Add navigation logic for ManagePatients
        System.Diagnostics.Debug.WriteLine("Navigating to ManagePatients...");
        App.MainFrame.Navigate(typeof(InsertPatient), null, new DrillInNavigationTransitionInfo());
    }
    private static void NavigateToWpisy()
    {
        App.MainFrame.Navigate(typeof(Wpisy), null, new DrillInNavigationTransitionInfo());
    }

    private static void Logout()
    {
        ClearUserData();
        ClearNavigationStack();
        App.MainFrame.Navigate(typeof(LoginPage));
    }

    private static void ClearUserData()
    {
        SharedData.pesel = null;
        SharedData.id = null;
        SharedData.rola = null;
        SharedData.PrimaryPesel = null;
    }
    private static void ClearNavigationStack()
    {
        App.MainFrame.BackStack.Clear();
    }
    private static void NavigateToRecepty()
    {
        App.MainFrame.Navigate(typeof(Recepty));
    }

    private static void NavigateToSkierowania()
    {
        App.MainFrame.Navigate(typeof(Skierowania));
    }

    private static void NavigateToWyniki()
    {
        App.MainFrame.Navigate(typeof(Wyniki));
    }

    private static void NavigateToPatientChoice()
    {
        App.MainFrame.Navigate(typeof(PeselChoice));
    }

    private static void NavigateToPanelGlowny()
    {
        App.MainFrame.Navigate(typeof(PanelGlowny));
    }
}