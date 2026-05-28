using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
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

    // Settings keys
    private const string KeyFontFamily = "FontFamily";
    private const string KeyFontSize = "FontSize";
    private const string KeyEditorPadding = "EditorPadding";
    private const string KeyWritingWidth = "WritingWidth";
    private const string KeyTextAlignment = "TextAlignment";
    private const string KeyWordWrap = "WordWrap";
    private const string KeyScrollbars = "Scrollbars";
    private const string KeySpellCheck = "SpellCheck";
    private const string KeyLineHeight = "LineHeight";
    private const string KeyLetterSpacing = "LetterSpacing";

    private Button? _fontFamilyPicker;
    private Button? _fontSizePicker;
    private Button? _editorPaddingPicker;
    private Button? _writingWidthPicker;
    private Button? _textAlignmentPicker;
    private Button? _lineHeightPicker;
    private Button? _letterSpacingPicker;
    private ToggleSwitch? _wordWrapToggle;
    private ToggleSwitch? _spellCheckToggle;
    private ToggleSwitch? _scrollbarsToggle;
    private StorageFile? _currentFile;

    private static ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;

    public MainPage()
    {
        InitializeComponent();
        BuildSettingsPage();
        LoadSettings();

        Loaded += (_, _) => Editor.Focus(FocusState.Programmatic);
    }

    // -------------------------------------------------------------------------
    // Settings persistence
    // -------------------------------------------------------------------------

    private void LoadSettings()
    {
        // Font family
        var fontFamily = Get(KeyFontFamily, InstrumentSerifDisplayName);
        var resolvedFont = ResolveFontFamily(fontFamily);
        Editor.FontFamily = new FontFamily(resolvedFont);
        SetPickerValue(_fontFamilyPicker, fontFamily);

        // Font size
        var fontSize = Get(KeyFontSize, 32.0);
        Editor.FontSize = fontSize;
        SyncFontSizePickerLabel(fontSize);

        // Editor padding
        var padding = Get(KeyEditorPadding, "Comfortable");
        Editor.Padding = ResolvePadding(padding);
        SetPickerValue(_editorPaddingPicker, padding);

        // Writing width
        var width = Get(KeyWritingWidth, "Full");
        ApplyWritingWidth(width);
        SetPickerValue(_writingWidthPicker, width);

        // Text alignment
        var alignment = Get(KeyTextAlignment, "Left");
        Editor.Document.GetText(TextGetOptions.None, out _);
        ApplyTextAlignment(alignment);
        SetPickerValue(_textAlignmentPicker, alignment);

        // Word wrap
        var wordWrap = Get(KeyWordWrap, true);
        Editor.TextWrapping = wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
        if (_wordWrapToggle is not null) _wordWrapToggle.IsOn = wordWrap;

        // Scrollbars
        var scrollbars = Get(KeyScrollbars, false);
        var scrollVisibility = scrollbars ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        ScrollViewer.SetHorizontalScrollBarVisibility(Editor, scrollVisibility);
        ScrollViewer.SetVerticalScrollBarVisibility(Editor, scrollVisibility);
        if (_scrollbarsToggle is not null) _scrollbarsToggle.IsOn = scrollbars;

        // Spell check
        var spellCheck = Get(KeySpellCheck, false);
        Editor.IsSpellCheckEnabled = spellCheck;
        if (_spellCheckToggle is not null) _spellCheckToggle.IsOn = spellCheck;

        // Line height
        var lineHeight = Get(KeyLineHeight, "Normal");
        ApplyLineHeight(lineHeight);
        SetPickerValue(_lineHeightPicker, lineHeight);

        // Letter spacing
        var letterSpacing = Get(KeyLetterSpacing, "Normal");
        ApplyLetterSpacing(letterSpacing);
        SetPickerValue(_letterSpacingPicker, letterSpacing);
    }

    private static T Get<T>(string key, T defaultValue)
    {
        if (Settings.Values.TryGetValue(key, out var raw) && raw is T value)
        {
            return value;
        }

        return defaultValue;
    }

    private static void Save(string key, object value) => Settings.Values[key] = value;

    // -------------------------------------------------------------------------
    // Build settings UI
    // -------------------------------------------------------------------------

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

        _lineHeightPicker = CreatePicker(["Tight", "Normal", "Relaxed", "Airy"], 1, LineHeightPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Line height", "Spacing between lines", _lineHeightPicker));

        _letterSpacingPicker = CreatePicker(["Tight", "Normal", "Wide", "Very Wide"], 1, LetterSpacingPicker_Changed);
        editorCard.Children.Add(CreateSettingRow("Letter spacing", "Space between characters", _letterSpacingPicker));

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

    private static StackPanel CreateCard() => new() { Spacing = 20 };

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

        var flyout = new MenuFlyout { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft };
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
        var toggle = new ToggleSwitch { IsOn = isOn, OnContent = "On", OffContent = "Off" };
        AutomationProperties.SetName(toggle, automationName);
        return toggle;
    }

    private static SolidColorBrush WhiteBrush() => new(Colors.White);

    private static FontFamily InstrumentSerif() => new(InstrumentSerifFontPath);

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    public void OpenSettingsPage()
    {
        if (SettingsView.Visibility == Visibility.Visible)
        {
            BackToEditor();
            return;
        }

        Editor.Visibility = Visibility.Collapsed;
        CountLabel.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;
    }

    private void BackToEditor()
    {
        SettingsView.Visibility = Visibility.Collapsed;
        Editor.Visibility = Visibility.Visible;
        CountLabel.Visibility = Visibility.Visible;

        // RichEditBox can reset to its default paragraph format when re-rendered
        // after a visibility change — re-apply to guarantee correctness.
        ReapplyAllFormatting();

        Editor.Focus(FocusState.Programmatic);
    }

    // -------------------------------------------------------------------------
    // File operations
    // -------------------------------------------------------------------------

    public async Task OpenFileAsync()
    {
        var picker = new FileOpenPicker();
        InitializePicker(picker);
        picker.FileTypeFilter.Add(".txt");

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        var text = await FileIO.ReadTextAsync(file);
        Editor.Document.SetText(TextSetOptions.None, text);
        _currentFile = file;

        // Re-apply formatting after loading new text
        ReapplyAllFormatting();
        BackToEditor();
    }

    public async Task SaveAsync()
    {
        if (_currentFile is null)
        {
            await SaveAsAsync();
            return;
        }

        Editor.Document.GetText(TextGetOptions.None, out var text);
        await FileIO.WriteTextAsync(_currentFile, text);
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
        if (file is null) return;

        Editor.Document.GetText(TextGetOptions.None, out var text);
        await FileIO.WriteTextAsync(file, text);
        _currentFile = file;
    }

    private static void InitializePicker(object picker)
    {
        if (App.MainWindow is null) return;
        WinRT.Interop.InitializeWithWindow.Initialize(
            picker,
            WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
    }

    // -------------------------------------------------------------------------
    // RichEditBox formatting helpers
    // -------------------------------------------------------------------------

    private void ApplyCharacterFormat(Action<ITextCharacterFormat> configure)
    {
        Editor.Document.GetText(TextGetOptions.None, out var text);
        var range = Editor.Document.GetRange(0, text.Length);
        configure(range.CharacterFormat);
    }

    private void ApplyParagraphFormat(Action<ITextParagraphFormat> configure)
    {
        // Use int.MaxValue to include all paragraphs and the trailing paragraph mark,
        // ensuring formatting applies even when the document is empty.
        var range = Editor.Document.GetRange(0, int.MaxValue);
        configure(range.ParagraphFormat);
    }

    /// <summary>Re-applies all character and paragraph formatting to freshly loaded text.</summary>
    private void ReapplyAllFormatting()
    {
        // Character
        var fontFamily = Get(KeyFontFamily, InstrumentSerifDisplayName);
        var fontSize = Get(KeyFontSize, 32.0);
        var letterSpacing = Get(KeyLetterSpacing, "Normal");
        ApplyCharacterFormat(f =>
        {
            f.Name = ResolveFontFamilyName(fontFamily);
            f.Size = (float)fontSize;
            f.Spacing = ResolveLetterSpacing(letterSpacing);
        });

        // Paragraph
        var lineHeight = Get(KeyLineHeight, "Normal");
        var alignment = Get(KeyTextAlignment, "Left");
        ApplyParagraphFormat(p =>
        {
            p.SetLineSpacing(LineSpacingRule.Multiple, ResolveLineHeight(lineHeight));
            p.Alignment = ResolveTextAlignment(alignment);
        });
    }

    // -------------------------------------------------------------------------
    // Setting change handlers
    // -------------------------------------------------------------------------

    private void FontFamilyPicker_Changed()
    {
        var name = GetSelectedPickerText(_fontFamilyPicker);
        var resolved = ResolveFontFamily(name);
        Editor.FontFamily = new FontFamily(resolved);
        ApplyCharacterFormat(f => f.Name = ResolveFontFamilyName(name));
        Save(KeyFontFamily, name);
    }

    private void FontSizePicker_Changed()
    {
        if (double.TryParse(GetSelectedPickerText(_fontSizePicker), out var fontSize))
        {
            Editor.FontSize = fontSize;
            ApplyCharacterFormat(f => f.Size = (float)fontSize);
            Save(KeyFontSize, fontSize);
        }
    }

    private void LineHeightPicker_Changed()
    {
        var value = GetSelectedPickerText(_lineHeightPicker);
        ApplyLineHeight(value);
        Save(KeyLineHeight, value);
    }

    private void LetterSpacingPicker_Changed()
    {
        var value = GetSelectedPickerText(_letterSpacingPicker);
        ApplyLetterSpacing(value);
        Save(KeyLetterSpacing, value);
    }

    private void EditorPaddingPicker_Changed()
    {
        var value = GetSelectedPickerText(_editorPaddingPicker);
        Editor.Padding = ResolvePadding(value);
        Save(KeyEditorPadding, value);
    }

    private void WritingWidthPicker_Changed()
    {
        var value = GetSelectedPickerText(_writingWidthPicker);
        ApplyWritingWidth(value);
        Save(KeyWritingWidth, value);
    }

    private void TextAlignmentPicker_Changed()
    {
        var value = GetSelectedPickerText(_textAlignmentPicker);
        ApplyTextAlignment(value);
        Save(KeyTextAlignment, value);
    }

    private void WordWrapToggle_Toggled(object sender, RoutedEventArgs args)
    {
        var on = _wordWrapToggle is not null && _wordWrapToggle.IsOn;
        Editor.TextWrapping = on ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Save(KeyWordWrap, on);
    }

    private void SpellCheckToggle_Toggled(object sender, RoutedEventArgs args)
    {
        var on = _spellCheckToggle is not null && _spellCheckToggle.IsOn;
        Editor.IsSpellCheckEnabled = on;
        Save(KeySpellCheck, on);
    }

    private void ScrollbarsToggle_Toggled(object sender, RoutedEventArgs args)
    {
        var on = _scrollbarsToggle is not null && _scrollbarsToggle.IsOn;
        var visibility = on ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        ScrollViewer.SetHorizontalScrollBarVisibility(Editor, visibility);
        ScrollViewer.SetVerticalScrollBarVisibility(Editor, visibility);
        Save(KeyScrollbars, on);
    }

    // -------------------------------------------------------------------------
    // Apply helpers
    // -------------------------------------------------------------------------

    private void ApplyLineHeight(string value)
    {
        var multiplier = ResolveLineHeight(value);
        ApplyParagraphFormat(p => p.SetLineSpacing(LineSpacingRule.Multiple, multiplier));
    }

    private void ApplyLetterSpacing(string value)
    {
        var spacing = ResolveLetterSpacing(value);
        ApplyCharacterFormat(f => f.Spacing = spacing);
    }

    private void ApplyWritingWidth(string value)
    {
        Editor.MaxWidth = value switch
        {
            "Comfortable" => 980,
            "Narrow" => 720,
            _ => double.PositiveInfinity,
        };
        Editor.HorizontalAlignment = Editor.MaxWidth == double.PositiveInfinity
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Center;
    }

    private void ApplyTextAlignment(string value)
    {
        var alignment = ResolveTextAlignment(value);
        ApplyParagraphFormat(p => p.Alignment = alignment);
    }

    // -------------------------------------------------------------------------
    // Resolve helpers
    // -------------------------------------------------------------------------

    private static float ResolveLineHeight(string value) => value switch
    {
        "Tight" => 1.0f,
        "Relaxed" => 1.8f,
        "Airy" => 2.2f,
        _ => 1.4f, // Normal
    };

    private static float ResolveLetterSpacing(string value) => value switch
    {
        "Tight" => -0.5f,
        "Wide" => 1.5f,
        "Very Wide" => 3.0f,
        _ => 0f, // Normal
    };

    private static Thickness ResolvePadding(string value) => value switch
    {
        "Compact" => new Thickness(20),
        "Wide" => new Thickness(56),
        _ => new Thickness(32),
    };

    private static ParagraphAlignment ResolveTextAlignment(string value) => value switch
    {
        "Center" => ParagraphAlignment.Center,
        "Right" => ParagraphAlignment.Right,
        _ => ParagraphAlignment.Left,
    };

    private static string ResolveFontFamily(string name)
    {
        return name == InstrumentSerifDisplayName ? InstrumentSerifFontPath : name;
    }

    private static string ResolveFontFamilyName(string name)
    {
        // ITextCharacterFormat.Name uses the plain family name, not the file path
        return name == InstrumentSerifDisplayName ? "Instrument Serif" : name;
    }

    // -------------------------------------------------------------------------
    // Ctrl+Scroll — font size
    // -------------------------------------------------------------------------

    private void Editor_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        var isCtrlDown = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Control)
            .HasFlag(CoreVirtualKeyStates.Down);

        if (!isCtrlDown) return;

        var delta = args.GetCurrentPoint(Editor).Properties.MouseWheelDelta;
        var step = delta > 0 ? 2.0 : -2.0;
        var newSize = Math.Clamp(Editor.FontSize + step, 8, 128);

        Editor.FontSize = newSize;
        ApplyCharacterFormat(f => f.Size = (float)newSize);
        SyncFontSizePickerLabel(newSize);
        Save(KeyFontSize, newSize);

        args.Handled = true;
    }

    // -------------------------------------------------------------------------
    // Word / character count
    // -------------------------------------------------------------------------

    private void Editor_TextChanged(object sender, RoutedEventArgs args)
    {
        Editor.Document.GetText(TextGetOptions.None, out var text);

        // RichEditBox appends a trailing \r — strip it for accurate counts
        var clean = text.TrimEnd('\r');
        var chars = clean.Length;
        var words = string.IsNullOrWhiteSpace(clean)
            ? 0
            : clean.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;

        CountLabel.Text = $"{words} words  ·  {chars} characters";
    }

    // -------------------------------------------------------------------------
    // Picker utilities
    // -------------------------------------------------------------------------

    private static string GetSelectedPickerText(Button? button) =>
        button?.Tag?.ToString() ?? string.Empty;

    private static void SetPickerValue(Button? button, string value)
    {
        if (button is null) return;
        button.Content = value;
        button.Tag = value;
        AutomationProperties.SetName(button, value);
    }

    private void SyncFontSizePickerLabel(double fontSize)
    {
        var label = fontSize.ToString("0");
        SetPickerValue(_fontSizePicker, label);
    }
}
