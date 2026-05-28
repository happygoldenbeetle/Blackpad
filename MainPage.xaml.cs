using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;

namespace BlackNotepad;

public sealed partial class MainPage : Page
{
    private const string InstrumentSerifDisplayName = "Instrument Serif";
    private const string InstrumentSerifFontPath = "Assets/Fonts/InstrumentSerif-Regular.ttf#Instrument Serif";

    private Button? _fontFamilyPicker;
    private Button? _fontSizePicker;
    private Button? _editorPaddingPicker;
    private Button? _writingWidthPicker;
    private Button? _textAlignmentPicker;
    private ToggleSwitch? _wordWrapToggle;
    private ToggleSwitch? _spellCheckToggle;
    private ToggleSwitch? _scrollbarsToggle;
    private StorageFile? _currentFile;

    public MainPage()
    {
        InitializeComponent();
        BuildSettingsPage();

        Loaded += (_, _) => Editor.Focus(FocusState.Programmatic);
    }

    private void BuildSettingsPage()
    {
        SettingsContent.Children.Clear();

        SettingsContent.Children.Add(new TextBlock
        {
            Text = "Text settings",
            Foreground = WhiteBrush(),
            FontFamily = InstrumentSerif(),
            FontSize = 40,
            Margin = new Thickness(0, 0, 0, 8),
            VerticalAlignment = VerticalAlignment.Center,
        });

        AddSectionTitle("Editor");
        var editorCard = CreateCard();
        _fontFamilyPicker = CreatePicker([InstrumentSerifDisplayName, "Segoe UI", "Sitka Subheading", "Consolas"], 0, FontFamilyPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Family", "Text font", _fontFamilyPicker));

        _fontSizePicker = CreatePicker(["16", "20", "24", "28", "32", "40", "48", "64"], 4, FontSizePicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Size", "Text size", _fontSizePicker));

        _editorPaddingPicker = CreatePicker(["Compact", "Comfortable", "Wide"], 1, EditorPaddingPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Margins", "Space around the writing area", _editorPaddingPicker));

        _writingWidthPicker = CreatePicker(["Full", "Comfortable", "Narrow"], 0, WritingWidthPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Writing width", "Limit the line length on wide windows", _writingWidthPicker));

        _textAlignmentPicker = CreatePicker(["Left", "Center", "Right"], 0, TextAlignmentPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Alignment", "How text sits on the page", _textAlignmentPicker));

        _wordWrapToggle = CreateToggle(true, "Word wrap");
        _wordWrapToggle.Toggled += WordWrapToggle_Toggled;
        editorCard.Children.Add(CreateSettingRow("Word wrap", "Fit text within the window", _wordWrapToggle));

        _scrollbarsToggle = CreateToggle(false, "Scrollbars");
        _scrollbarsToggle.Toggled += ScrollbarsToggle_Toggled;
        editorCard.Children.Add(CreateSettingRow("Scrollbars", "Show scrollbars while writing", _scrollbarsToggle));

        SettingsContent.Children.Add(WrapCard(editorCard));

        AddSectionTitle("Spelling");
        var spellingCard = CreateCard();
        _spellCheckToggle = CreateToggle(false, "Spell check");
        _spellCheckToggle.Toggled += SpellCheckToggle_Toggled;
        spellingCard.Children.Add(CreateSettingRow("Spell check", "Check spelling while typing", _spellCheckToggle));
        SettingsContent.Children.Add(WrapCard(spellingCard));
    }

    private void AddSectionTitle(string title)
    {
        SettingsContent.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = InstrumentSerif(),
            FontSize = 28,
            Foreground = WhiteBrush(),
            Margin = new Thickness(0, 20, 0, 4),
        });
    }

    private static StackPanel CreateCard()
    {
        return new StackPanel { Spacing = 20 };
    }

    private static Border WrapCard(UIElement child)
    {
        return new Border
        {
            Padding = new Thickness(20),
            Background = new SolidColorBrush(ColorHelper.FromArgb(255, 42, 38, 36)),
            CornerRadius = new CornerRadius(6),
            Child = child,
        };
    }

    private static Grid CreateSettingRow(string title, string subtitle, FrameworkElement control)
    {
        var row = new Grid { ColumnSpacing = 24 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });

        var labels = new StackPanel();
        labels.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = InstrumentSerif(),
            FontSize = 18,
            Foreground = WhiteBrush(),
        });
        labels.Children.Add(new TextBlock
        {
            Text = subtitle,
            Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 216, 210, 206)),
        });

        Grid.SetColumn(labels, 0);
        Grid.SetColumn(control, 1);
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(labels);
        row.Children.Add(control);
        return row;
    }

    private static Button CreatePicker(string[] values, int selectedIndex, Action onChanged)
    {
        var button = new Button
        {
            Content = values[selectedIndex],
            Tag = values[selectedIndex],
            HorizontalContentAlignment = HorizontalAlignment.Left,
        };
        AutomationProperties.SetName(button, values[selectedIndex]);

        var flyout = new MenuFlyout
        {
            Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft,
        };
        foreach (var value in values)
        {
            var item = new MenuFlyoutItem { Text = value };
            item.Click += (_, _) =>
            {
                button.Content = value;
                button.Tag = value;
                AutomationProperties.SetName(button, value);
                onChanged();
            };
            flyout.Items.Add(item);
        }

        button.Flyout = flyout;
        return button;
    }

    private static ToggleSwitch CreateToggle(bool isOn, string automationName)
    {
        var toggle = new ToggleSwitch
        {
            IsOn = isOn,
            OnContent = "On",
            OffContent = "Off",
        };
        AutomationProperties.SetName(toggle, automationName);
        return toggle;
    }

    private static SolidColorBrush WhiteBrush()
    {
        return new SolidColorBrush(Colors.White);
    }

    private static FontFamily InstrumentSerif()
    {
        return new FontFamily(InstrumentSerifFontPath);
    }

    public void OpenSettingsPage()
    {
        if (SettingsView.Visibility == Visibility.Visible)
        {
            BackToEditor();
            return;
        }

        Editor.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;
    }

    public async Task OpenFileAsync()
    {
        var picker = new FileOpenPicker();
        InitializePicker(picker);
        picker.FileTypeFilter.Add(".txt");

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        Editor.Text = await FileIO.ReadTextAsync(file);
        _currentFile = file;
        BackToEditor();
    }

    public async Task SaveAsync()
    {
        if (_currentFile is null)
        {
            await SaveAsAsync();
            return;
        }

        await FileIO.WriteTextAsync(_currentFile, Editor.Text);
    }

    public async Task SaveAsAsync()
    {
        var picker = new FileSavePicker
        {
            SuggestedFileName = _currentFile is null
                ? "Untitled"
                : Path.GetFileNameWithoutExtension(_currentFile.Name),
        };
        InitializePicker(picker);
        picker.FileTypeChoices.Add("Text", [".txt"]);
        picker.DefaultFileExtension = ".txt";

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        await FileIO.WriteTextAsync(file, Editor.Text);
        _currentFile = file;
    }

    private static void InitializePicker(object picker)
    {
        if (App.MainWindow is null)
        {
            return;
        }

        WinRT.Interop.InitializeWithWindow.Initialize(
            picker,
            WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
    }

    private void BackToEditor()
    {
        SettingsView.Visibility = Visibility.Collapsed;
        Editor.Visibility = Visibility.Visible;
        Editor.Focus(FocusState.Programmatic);
    }

    private void FontFamilyPicker_Changed()
    {
        var fontFamily = ResolveFontFamily(GetSelectedPickerText(_fontFamilyPicker));
        if (!string.IsNullOrWhiteSpace(fontFamily))
        {
            Editor.FontFamily = new FontFamily(fontFamily);
        }
    }

    private void FontSizePicker_Changed()
    {
        if (double.TryParse(GetSelectedPickerText(_fontSizePicker), out var fontSize))
        {
            Editor.FontSize = fontSize;
        }
    }

    private void EditorPaddingPicker_Changed()
    {
        Editor.Padding = GetSelectedPickerText(_editorPaddingPicker) switch
        {
            "Compact" => new Thickness(20),
            "Wide" => new Thickness(56),
            _ => new Thickness(32),
        };
    }

    private void WritingWidthPicker_Changed()
    {
        Editor.MaxWidth = GetSelectedPickerText(_writingWidthPicker) switch
        {
            "Comfortable" => 980,
            "Narrow" => 720,
            _ => double.PositiveInfinity,
        };
        Editor.HorizontalAlignment = Editor.MaxWidth == double.PositiveInfinity
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Center;
    }

    private void TextAlignmentPicker_Changed()
    {
        Editor.TextAlignment = GetSelectedPickerText(_textAlignmentPicker) switch
        {
            "Center" => TextAlignment.Center,
            "Right" => TextAlignment.Right,
            _ => TextAlignment.Left,
        };
    }

    private void WordWrapToggle_Toggled(object sender, RoutedEventArgs args)
    {
        Editor.TextWrapping = _wordWrapToggle is not null && _wordWrapToggle.IsOn
            ? TextWrapping.Wrap
            : TextWrapping.NoWrap;
    }

    private void SpellCheckToggle_Toggled(object sender, RoutedEventArgs args)
    {
        Editor.IsSpellCheckEnabled = _spellCheckToggle is not null && _spellCheckToggle.IsOn;
    }

    private void ScrollbarsToggle_Toggled(object sender, RoutedEventArgs args)
    {
        var visibility = _scrollbarsToggle is not null && _scrollbarsToggle.IsOn
            ? ScrollBarVisibility.Auto
            : ScrollBarVisibility.Hidden;
        ScrollViewer.SetHorizontalScrollBarVisibility(Editor, visibility);
        ScrollViewer.SetVerticalScrollBarVisibility(Editor, visibility);
    }

    private static string GetSelectedPickerText(Button? button)
    {
        return button?.Tag?.ToString() ?? string.Empty;
    }

    private static string ResolveFontFamily(string fontFamily)
    {
        return fontFamily == InstrumentSerifDisplayName
            ? InstrumentSerifFontPath
            : fontFamily;
    }

    private void Editor_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        var isCtrlDown = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Control)
            .HasFlag(CoreVirtualKeyStates.Down);

        if (!isCtrlDown)
        {
            return;
        }

        var delta = args.GetCurrentPoint(Editor).Properties.MouseWheelDelta;
        var step = delta > 0 ? 2.0 : -2.0;
        var newSize = Math.Clamp(Editor.FontSize + step, 8, 128);

        Editor.FontSize = newSize;
        SyncFontSizePickerLabel(newSize);

        // Prevent the ScrollViewer inside the TextBox from also scrolling
        args.Handled = true;
    }

    private void SyncFontSizePickerLabel(double fontSize)
    {
        if (_fontSizePicker is null)
        {
            return;
        }

        var label = fontSize.ToString("0");
        _fontSizePicker.Content = label;
        _fontSizePicker.Tag = label;
        AutomationProperties.SetName(_fontSizePicker, label);
    }
}
