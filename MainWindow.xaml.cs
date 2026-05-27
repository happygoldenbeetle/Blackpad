using Microsoft.UI.Windowing;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;

namespace BlackNotepad;

public sealed partial class MainWindow : Window
{
    private bool _isFullScreen;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon("Assets/AppIcon.ico");
        ConfigureBorderlessPresenter();

        RootFrame.AddHandler(
            UIElement.KeyDownEvent,
            new KeyEventHandler(RootFrame_KeyDown),
            true);

        RootFrame.Navigate(typeof(MainPage));
    }

    private void ConfigureBorderlessPresenter()
    {
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
        }
    }

    private async void RootFrame_KeyDown(object sender, KeyRoutedEventArgs args)
    {
        if (args.Key == VirtualKey.F11)
        {
            ToggleFullScreen();
            args.Handled = true;
            return;
        }

        if (RootFrame.Content is not MainPage mainPage)
        {
            return;
        }

        if (args.Key == VirtualKey.E && IsKeyDown(VirtualKey.Control))
        {
            mainPage.OpenSettingsPage();
            args.Handled = true;
            return;
        }

        if (args.Key == VirtualKey.S && IsKeyDown(VirtualKey.Control))
        {
            if (IsKeyDown(VirtualKey.Shift))
            {
                await mainPage.SaveAsAsync();
            }
            else
            {
                await mainPage.SaveAsync();
            }

            args.Handled = true;
            return;
        }

        if (args.Key == VirtualKey.O && IsKeyDown(VirtualKey.Control))
        {
            await mainPage.OpenFileAsync();
            args.Handled = true;
        }
    }

    private static bool IsKeyDown(VirtualKey key)
    {
        return InputKeyboardSource
            .GetKeyStateForCurrentThread(key)
            .HasFlag(CoreVirtualKeyStates.Down);
    }

    private void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Default);
            ConfigureBorderlessPresenter();
            _isFullScreen = false;
        }
        else
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            _isFullScreen = true;
        }
    }
}
