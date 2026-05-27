using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BlackNotepad;

public sealed partial class MainPage : Page
{
    private const string PreviewSampleMarkdown = "# Markdown preview\n\nWrite **bold**, *italic*, `code`, bullets, quotes, and headings.\n\n- Quiet source\n- Live preview\n\n> Still just black when you want it.";

    private ComboBox? _viewModeComboBox;
    private ToggleSwitch? _inlineMarkdownToggle;
    private ComboBox? _fontFamilyComboBox;
    private ComboBox? _fontSizeComboBox;
    private ComboBox? _previewFontSizeComboBox;
    private ToggleSwitch? _wordWrapToggle;
    private ToggleSwitch? _spellCheckToggle;
    private RichTextBlock? _previewSample;
    private StorageFile? _currentFile;

    public MainPage()
    {
        InitializeComponent();
        BuildSettingsPage();

        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs args)
    {
        RenderMarkdownPreview();
        RenderMarkdown(PreviewSampleMarkdown, _previewSample);
        Editor.Focus(FocusState.Programmatic);
    }

    private void BuildSettingsPage()
    {
        SettingsContent.Children.Clear();

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
        };
        header.Children.Add(new Button
        {
            Content = "Back",
            Command = new RelayCommand(BackToEditor),
        });
        header.Children.Add(new TextBlock
        {
            Text = "Markdown settings",
            Foreground = WhiteBrush(),
            FontFamily = InstrumentSerif(),
            FontSize = 28,
            VerticalAlignment = VerticalAlignment.Center,
        });
        SettingsContent.Children.Add(header);

        AddSectionTitle("Markdown");
        var markdownCard = CreateCard();
        _viewModeComboBox = CreateComboBox(["Editor only", "Split preview", "Preview only"], 0);
        _viewModeComboBox.SelectionChanged += ViewModeComboBox_SelectionChanged;
        markdownCard.Children.Add(CreateSettingRow("View mode", "Choose how Markdown is shown while writing", _viewModeComboBox));

        _inlineMarkdownToggle = CreateToggle(true, "Render inline Markdown");
        _inlineMarkdownToggle.Toggled += MarkdownRenderingToggle_Toggled;
        markdownCard.Children.Add(CreateSettingRow("Render inline Markdown", "Bold, italic, and inline code in preview", _inlineMarkdownToggle));
        SettingsContent.Children.Add(WrapCard(markdownCard));

        AddSectionTitle("Editor");
        var editorCard = CreateCard();
        _fontFamilyComboBox = CreateComboBox(["Instrument Serif", "Segoe UI", "Sitka Subheading", "Consolas"], 0);
        _fontFamilyComboBox.SelectionChanged += FontFamilyComboBox_SelectionChanged;
        editorCard.Children.Add(CreateSettingRow("Family", "Source text font", _fontFamilyComboBox));

        _fontSizeComboBox = CreateComboBox(["16", "20", "24", "28", "32", "40", "48", "64"], 4);
        _fontSizeComboBox.SelectionChanged += FontSizeComboBox_SelectionChanged;
        editorCard.Children.Add(CreateSettingRow("Size", "Source text size", _fontSizeComboBox));

        _wordWrapToggle = CreateToggle(true, "Word wrap");
        _wordWrapToggle.Toggled += WordWrapToggle_Toggled;
        editorCard.Children.Add(CreateSettingRow("Word wrap", "Fit Markdown source within the window", _wordWrapToggle));
        SettingsContent.Children.Add(WrapCard(editorCard));

        AddSectionTitle("Preview");
        var previewCard = CreateCard();
        _previewFontSizeComboBox = CreateComboBox(["18", "22", "24", "28", "32", "40"], 3);
        _previewFontSizeComboBox.SelectionChanged += PreviewFontSizeComboBox_SelectionChanged;
        previewCard.Children.Add(CreateSettingRow("Preview size", "Rendered Markdown text size", _previewFontSizeComboBox));

        _previewSample = new RichTextBlock
        {
            FontFamily = new FontFamily("Instrument Serif"),
            FontSize = 28,
            Foreground = WhiteBrush(),
            TextWrapping = TextWrapping.Wrap,
        };
        previewCard.Children.Add(new Border
        {
            Padding = new Thickness(12, 24, 12, 24),
            Background = new SolidColorBrush(ColorHelper.FromArgb(255, 36, 33, 31)),
            CornerRadius = new CornerRadius(4),
            Child = _previewSample,
        });
        SettingsContent.Children.Add(WrapCard(previewCard));

        AddSectionTitle("Spelling");
        var spellingCard = CreateCard();
        _spellCheckToggle = CreateToggle(false, "Spell check");
        _spellCheckToggle.Toggled += SpellCheckToggle_Toggled;
        spellingCard.Children.Add(CreateSettingRow("Spell check", "Check spelling in Markdown source", _spellCheckToggle));
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

    private static ComboBox CreateComboBox(string[] values, int selectedIndex)
    {
        var comboBox = new ComboBox
        {
            SelectedIndex = selectedIndex,
        };

        foreach (var value in values)
        {
            comboBox.Items.Add(new ComboBoxItem { Content = value });
        }

        return comboBox;
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
        return new FontFamily("Instrument Serif");
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs args)
    {
        RenderMarkdownPreview();
    }

    public void OpenSettingsPage()
    {
        MarkdownWorkspace.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;
    }

    public async Task OpenFileAsync()
    {
        var picker = new FileOpenPicker();
        InitializePicker(picker);
        picker.FileTypeFilter.Add(".md");
        picker.FileTypeFilter.Add(".markdown");
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
        picker.FileTypeChoices.Add("Markdown", [".md"]);
        picker.FileTypeChoices.Add("Text", [".txt"]);
        picker.DefaultFileExtension = ".md";

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
        MarkdownWorkspace.Visibility = Visibility.Visible;
        ApplyViewMode();

        if (Editor.Visibility == Visibility.Visible)
        {
            Editor.Focus(FocusState.Programmatic);
        }
    }

    private void ViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        ApplyViewMode();
    }

    private void ApplyViewMode()
    {
        var mode = GetSelectedComboBoxText(_viewModeComboBox);
        if (mode == "Split preview")
        {
            Editor.Visibility = Visibility.Visible;
            PreviewPane.Visibility = Visibility.Visible;
            EditorColumn.Width = new GridLength(1, GridUnitType.Star);
            PreviewColumn.Width = new GridLength(1, GridUnitType.Star);
        }
        else if (mode == "Preview only")
        {
            Editor.Visibility = Visibility.Collapsed;
            PreviewPane.Visibility = Visibility.Visible;
            EditorColumn.Width = new GridLength(0);
            PreviewColumn.Width = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            Editor.Visibility = Visibility.Visible;
            PreviewPane.Visibility = Visibility.Collapsed;
            EditorColumn.Width = new GridLength(1, GridUnitType.Star);
            PreviewColumn.Width = new GridLength(0);
        }

        RenderMarkdownPreview();
    }

    private void MarkdownRenderingToggle_Toggled(object sender, RoutedEventArgs args)
    {
        RenderMarkdownPreview();
        RenderMarkdown(PreviewSampleMarkdown, _previewSample);
    }

    private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        var fontFamily = GetSelectedComboBoxText(_fontFamilyComboBox);
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            return;
        }

        Editor.FontFamily = new FontFamily(fontFamily);
        MarkdownPreview.FontFamily = new FontFamily(fontFamily);
        if (_previewSample is not null)
        {
            _previewSample.FontFamily = new FontFamily(fontFamily);
        }
    }

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (double.TryParse(GetSelectedComboBoxText(_fontSizeComboBox), out var fontSize))
        {
            Editor.FontSize = fontSize;
        }
    }

    private void PreviewFontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (!double.TryParse(GetSelectedComboBoxText(_previewFontSizeComboBox), out var fontSize))
        {
            return;
        }

        MarkdownPreview.FontSize = fontSize;
        if (_previewSample is not null)
        {
            _previewSample.FontSize = fontSize;
        }

        RenderMarkdownPreview();
        RenderMarkdown(PreviewSampleMarkdown, _previewSample);
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

    private void RenderMarkdownPreview()
    {
        RenderMarkdown(Editor.Text, MarkdownPreview);
    }

    private void RenderMarkdown(string markdown, RichTextBlock? target)
    {
        if (target is null)
        {
            return;
        }

        target.Blocks.Clear();
        if (string.IsNullOrEmpty(markdown))
        {
            return;
        }

        var normalized = markdown.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var inCodeBlock = false;
        var codeLines = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCodeBlock)
                {
                    AddCodeBlock(target, string.Join(Environment.NewLine, codeLines));
                    codeLines.Clear();
                    inCodeBlock = false;
                }
                else
                {
                    inCodeBlock = true;
                }

                continue;
            }

            if (inCodeBlock)
            {
                codeLines.Add(rawLine);
                continue;
            }

            AddMarkdownLine(target, line);
        }

        if (inCodeBlock)
        {
            AddCodeBlock(target, string.Join(Environment.NewLine, codeLines));
        }
    }

    private void AddMarkdownLine(RichTextBlock target, string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            target.Blocks.Add(new Paragraph());
            return;
        }

        var trimmed = line.TrimStart();
        if (TryGetHeadingLevel(trimmed, out var headingLevel, out var headingText))
        {
            var heading = new Paragraph
            {
                FontSize = target.FontSize + Math.Max(4, 24 - (headingLevel * 3)),
                FontWeight = FontWeights.SemiBold,
            };
            AddInlineMarkdown(headingText, heading.Inlines);
            target.Blocks.Add(heading);
            return;
        }

        if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
        {
            var bullet = new Paragraph();
            bullet.Inlines.Add(new Run { Text = "- " });
            AddInlineMarkdown(trimmed[2..], bullet.Inlines);
            target.Blocks.Add(bullet);
            return;
        }

        if (trimmed.StartsWith("> ", StringComparison.Ordinal))
        {
            var quote = new Paragraph
            {
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 216, 210, 206)),
            };
            quote.Inlines.Add(new Run { Text = "> " });
            AddInlineMarkdown(trimmed[2..], quote.Inlines);
            target.Blocks.Add(quote);
            return;
        }

        var paragraph = new Paragraph();
        AddInlineMarkdown(line, paragraph.Inlines);
        target.Blocks.Add(paragraph);
    }

    private void AddCodeBlock(RichTextBlock target, string code)
    {
        var paragraph = new Paragraph
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = Math.Max(12, target.FontSize * 0.72),
            Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 225, 225, 225)),
        };
        paragraph.Inlines.Add(new Run { Text = code });
        target.Blocks.Add(paragraph);
    }

    private void AddInlineMarkdown(string text, InlineCollection inlines)
    {
        if (_inlineMarkdownToggle is not null && !_inlineMarkdownToggle.IsOn)
        {
            inlines.Add(new Run { Text = text });
            return;
        }

        var index = 0;
        while (index < text.Length)
        {
            var codeStart = text.IndexOf('`', index);
            var boldStart = text.IndexOf("**", index, StringComparison.Ordinal);
            var italicStart = text.IndexOf('*', index);
            var next = GetNextMarker(codeStart, boldStart, italicStart);

            if (next < 0)
            {
                inlines.Add(new Run { Text = text[index..] });
                return;
            }

            if (next > index)
            {
                inlines.Add(new Run { Text = text[index..next] });
            }

            if (next == codeStart)
            {
                index = AddDelimitedInline(text, inlines, next, "`", InlineKind.Code);
            }
            else if (next == boldStart)
            {
                index = AddDelimitedInline(text, inlines, next, "**", InlineKind.Bold);
            }
            else
            {
                index = AddDelimitedInline(text, inlines, next, "*", InlineKind.Italic);
            }
        }
    }

    private static int AddDelimitedInline(string text, InlineCollection inlines, int start, string delimiter, InlineKind kind)
    {
        var contentStart = start + delimiter.Length;
        var end = text.IndexOf(delimiter, contentStart, StringComparison.Ordinal);
        if (end < 0)
        {
            inlines.Add(new Run { Text = text[start..] });
            return text.Length;
        }

        var content = text[contentStart..end];
        if (kind == InlineKind.Bold)
        {
            var bold = new Bold();
            bold.Inlines.Add(new Run { Text = content });
            inlines.Add(bold);
        }
        else if (kind == InlineKind.Italic)
        {
            var italic = new Italic();
            italic.Inlines.Add(new Run { Text = content });
            inlines.Add(italic);
        }
        else
        {
            inlines.Add(new Run
            {
                Text = content,
                FontFamily = new FontFamily("Consolas"),
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 225, 225, 225)),
            });
        }

        return end + delimiter.Length;
    }

    private static int GetNextMarker(params int[] markerIndexes)
    {
        return markerIndexes.Where(index => index >= 0).DefaultIfEmpty(-1).Min();
    }

    private static bool TryGetHeadingLevel(string line, out int level, out string text)
    {
        level = 0;
        while (level < line.Length && level < 6 && line[level] == '#')
        {
            level++;
        }

        if (level == 0 || level >= line.Length || line[level] != ' ')
        {
            level = 0;
            text = string.Empty;
            return false;
        }

        text = line[(level + 1)..];
        return true;
    }

    private static string GetSelectedComboBoxText(ComboBox? comboBox)
    {
        return comboBox?.SelectedItem is ComboBoxItem selectedItem
            ? selectedItem.Content?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static bool ContainsSettingsTrigger(string text)
    {
        return text
            .Split([' ', '\r', '\n', '\t', '.', ',', ';', ':', '!', '?', '#', '*', '`', '-', '>'], StringSplitOptions.RemoveEmptyEntries)
            .Any(IsSettingsTrigger);
    }

    private static bool IsSettingsTrigger(string text)
    {
        return text is "s" or "S" or "settings" or "Settings";
    }

    private string GetWordAtCaret()
    {
        var text = Editor.Text;
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var index = Math.Clamp(Editor.SelectionStart, 0, text.Length - 1);
        if (index > 0 && !IsWordCharacter(text[index]) && IsWordCharacter(text[index - 1]))
        {
            index--;
        }

        if (!IsWordCharacter(text[index]))
        {
            return string.Empty;
        }

        var start = index;
        while (start > 0 && IsWordCharacter(text[start - 1]))
        {
            start--;
        }

        var end = index;
        while (end + 1 < text.Length && IsWordCharacter(text[end + 1]))
        {
            end++;
        }

        return text[start..(end + 1)];
    }

    private static bool IsWordCharacter(char value)
    {
        return char.IsLetter(value);
    }

    private sealed class RelayCommand(Action execute) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            execute();
        }
    }

    private enum InlineKind
    {
        Bold,
        Italic,
        Code,
    }
}
