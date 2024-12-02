using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ResearchUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private StringBuilder _consoleOutput; // Shared memory for console output
    private Python _python;

    public MainWindow()
    {
        InitializeComponent();


        // Set default values for the input fields
        DataDirTextBox.Text = "Data";
        VideoDirTextBox.Text = "Data/video";
        ExportDirTextBox.Text = "Data/export";
        RegenerateCheckBox.IsChecked = true;
        ProcessIndividuallyCheckBox.IsChecked = false;
        UseMaskCheckBox.IsChecked = false;
        MaskModelComboBox.SelectedIndex = 0; // "Skymask"
        MaskProcessorComboBox.SelectedIndex = 2; // "gpu"
        DropRatioTextBox.Text = "0.95";
        QualityThresholdTextBox.Text = "0.5";
        DebugCheckBox.IsChecked = false;
        ResolutionTextBox.Text = "3840x2160";
        KeypointCountTextBox.Text = "1600000";
        IterativeMatchCheckBox.IsChecked = false;
        AlignmentVersionTextBox.Text = "0";
        NormalizeBrightnessCheckBox.IsChecked = false;

        // Set defaults for clipping options
        QuantileTextBox.Text = "0";
        BufferPercentTextBox.Text = "0.1";
        ConfidenceThresholdTextBox.Text = "0";


        // Initialize the shared console output
        _consoleOutput = new StringBuilder();

        // Bind the Console TextBox controls to the shared console output
        ConsoleOutputTextBox.Text = _consoleOutput.ToString();
        ConsoleOutputTextBoxOnly.Text = _consoleOutput.ToString();

        // Redirect Console output
        Console.SetOut(new TextBoxWriter(this));
        Console.SetError(new TextBoxWriter(this));
        
        _python = new Python();
        
        // run the startup sequence asynchronously
        Task.Run(StartupSequence);
    }


    private async void StartupSequence()
    {
        // print assembly version and build date
        Console.WriteLine("VidTree UI");
        Console.WriteLine($"Assembly: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");
        Console.WriteLine($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
        Console.WriteLine(
            $"Build Date: {File.GetCreationTime(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
        Console.WriteLine();

        await _python.RunProcess("Scripts/setup.py");
    }


    private void RunScriptButton_Click(object sender, RoutedEventArgs e)
    {
        string dataDir = DataDirTextBox.Text;
        string videoDir = VideoDirTextBox.Text;
        string exportDir = ExportDirTextBox.Text;

        // Other inputs omitted for brevity

        string arguments = $@"--data_dir {dataDir} --video_dir {videoDir} --export_dir {exportDir}";

        _python.RunProcess(arguments).Wait();
    }


    private void AppendConsoleOutput(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Update the shared console output
        _consoleOutput.AppendLine(text);

        // Refresh both TextBox controls
        Dispatcher.Invoke(() =>
        {
            ConsoleOutputTextBox.Text = _consoleOutput.ToString();
            ConsoleOutputTextBoxOnly.Text = _consoleOutput.ToString();

            // Scroll to the bottom for both TextBoxes
            ConsoleOutputTextBox.ScrollToEnd();
            ConsoleOutputTextBoxOnly.ScrollToEnd();
        });
    }

    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var textBoxName = button?.Tag.ToString();
        if (textBoxName == null) return;
        var textBox = FindName(textBoxName) as TextBox;

        var dialog = new OpenFolderDialog
        {
            Title = "Select a folder",
            InitialDirectory = textBox?.Text
        };

        if (dialog.ShowDialog() != true) return;
        string? folderPath = System.IO.Path.GetDirectoryName(dialog.FolderName);
        if (textBox != null) textBox.Text = folderPath;
    }

    private class TextBoxWriter : TextWriter
    {
        private readonly MainWindow _window;

        public TextBoxWriter(MainWindow window)
        {
            _window = window;
        }

        public override void Write(char value)
        {
            _window.AppendConsoleOutput(value.ToString());
        }

        public override void Write(string value)
        {
            _window.AppendConsoleOutput(value);
        }

        public override void WriteLine(string value)
        {
            _window.AppendConsoleOutput(value);
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}