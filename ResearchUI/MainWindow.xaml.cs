using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PythonBindings;
using ResearchUI.Model;

namespace ResearchUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private StringBuilder _consoleOutput; // Shared memory for console output
    private PythonEnvironment _pythonEnvironment;

    public FiducialConstraints FiducialConstraints { get; set; } = new();

    public ICommand AddPairCommand { get; }
    public ICommand RemovePairCommand { get; }
    public ICommand AddPositionCommand { get; }
    public ICommand RemovePositionCommand { get; }


    private bool _isConsoleTabActive;

    public bool IsConsoleTabActive
    {
        get => _isConsoleTabActive;
        set
        {
            _isConsoleTabActive = value;
            OnPropertyChanged(nameof(IsConsoleTabActive));
        }
    }


    public MainWindow()
    {
        InitializeComponent();


        // Set default values for the input fields
        DataDirTextBox.Text = "Data";
        VideoDirTextBox.Text = "Data/video";
        ExportDirTextBox.Text = "Data/export";
        ConfigDirTextBox.Text = "Data/config";
        RegenerateCheckBox.IsChecked = true;
        ProcessIndividuallyCheckBox.IsChecked = false;
        UseMaskCheckBox.IsChecked = false;
        MaskModelComboBox.SelectedIndex = 0; // "Skymask"
        MaskProcessorComboBox.SelectedIndex = 2; // "gpu"
        DropRatioTextBox.Text = "0.95";
        QualityThresholdTextBox.Text = "0.5";
        DebugCheckBox.IsChecked = false;
        ResolutionWidthTextBox.Text = "3840";
        ResolutionHeightTextBox.Text = "2160";
        TiepointCountTextBox.Text = "0";
        KeypointCountTextBox.Text = "1600000";
        AlignmentVersionComboBox.SelectedIndex = 0; // "Disabled"
        NormalizeBrightnessCheckBox.IsChecked = false;

        // Set defaults for clipping options
        QuantileTextBox.Text = "0";
        BufferPercentTextBox.Text = "0.1";
        ConfidenceThresholdTextBox.Text = "0";


        // Set default values for collection parameters
        SensorCorrectionCheckbox.IsChecked = false;
        FiducialConstraintCheckbox.IsChecked = true;

        RollingShutterCombobox.SelectedIndex = 0; // "Disabled"
        SensorTypeComboBox.SelectedIndex = 0; // "Unknown"

        PixelHeightTextBox.Text = "0.0019";
        PixelWidthTextBox.Text = "0.0019";


        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 1", Target2 = "target 2", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 1", Target2 = "target 3", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 1", Target2 = "target 4", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 1", Target2 = "target 5", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 2", Target2 = "target 3", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 2", Target2 = "target 4", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 2", Target2 = "target 5", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 3", Target2 = "target 4", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 3", Target2 = "target 5", Distance = 0.185 });
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "target 4", Target2 = "target 5", Distance = 0.185 });


        FiducialConstraints.Position.Add(new Position
            { Target = "target 1", X = 0.0, Y = 0.0925, Z = 0.1045, Accuracy = 0.01 });
        
        FiducialConstraints.Position.Add(new Position
            { Target = "target 2", X = 0.185, Y = 0.0925, Z = 0.1045, Accuracy = 0.01 });
        
        FiducialConstraints.Position.Add(new Position
            { Target = "target 3", X = 0.0925, Y = 0.0925, Z = 0.209, Accuracy = 0.01 });
        
                
        FiducialConstraints.Position.Add(new Position
            { Target = "target 4", X = 0.0925, Y = 0.0, Z = 0.1045, Accuracy = 0.01 });
        
        FiducialConstraints.Position.Add(new Position
            { Target = "target 5", X = 0.0925, Y = 0.185, Z = 0.1045, Accuracy = 0.01 });


        AddPairCommand = new RelayCommand(AddPair);
        RemovePairCommand = new RelayCommand<Pair>(RemovePair);
        AddPositionCommand = new RelayCommand(AddPosition);
        RemovePositionCommand = new RelayCommand<Position>(RemovePosition);


        // Set DataContext for binding
        DataContext = this;


        // Initialize the shared console output
        _consoleOutput = new StringBuilder();

        // Bind the Console TextBox controls to the shared console output
        ConsoleOutputTextBox.Text = _consoleOutput.ToString();
        ConsoleOutputTextBoxOnly.Text = _consoleOutput.ToString();

        // Redirect Console output
        Console.SetOut(new TextBoxWriter(this));
        Console.SetError(new TextBoxWriter(this));

        // run the startup sequence asynchronously
        Task.Run(StartupSequence);
    }

    private void AddPair()
    {
        FiducialConstraints.Pairs.Add(new Pair { Target1 = "", Target2 = "", Distance = 0 });
    }

    private void RemovePair(Pair pair)
    {
        FiducialConstraints.Pairs.Remove(pair);
    }

    private void AddPosition()
    {
        FiducialConstraints.Position.Add(new Position { Target = "", X = 0, Y = 0, Z = 0, Accuracy = 0 });
    }

    private void RemovePosition(Position position)
    {
        FiducialConstraints.Position.Remove(position);
    }


    private void ProcessFiducialConstraints()
    {
        if (FiducialConstraints.Enabled)
        {
            foreach (var pair in FiducialConstraints.Pairs)
            {
                Console.WriteLine($"Pair: {pair.Target1}-{pair.Target2}, Distance: {pair.Distance}");
            }

            foreach (var position in FiducialConstraints.Position)
            {
                Console.WriteLine(
                    $"Position: {position.Target} (X: {position.X}, Y: {position.Y}, Z: {position.Z}, Accuracy: {position.Accuracy})");
            }
        }
        else
        {
            Console.WriteLine("Fiducial Constraints are disabled.");
        }
    }


    private async void StartupSequence()
    {
        Console.WriteLine("Starting up...");
        _pythonEnvironment = await PythonEnvironment.SetupPython("Python", "python-setup.json");
        Console.WriteLine("Python environment setup complete.");
        
        
        // print assembly version and build date
        Console.WriteLine("VidTree UI");
        Console.WriteLine($"Assembly: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");
        Console.WriteLine($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
        Console.WriteLine(
            $"Build Date: {File.GetCreationTime(System.Reflection.Assembly.GetExecutingAssembly().Location)}");
        Console.WriteLine();

        await _pythonEnvironment.RunScript("--version");

        await _pythonEnvironment.RunScript("Scripts/setup.py");
    }

    // Allow only numeric input (integers)
    private void NumericInputOnly(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !NumericOnlyRegex().IsMatch(e.Text);
    }

    // Allow only float input (decimal numbers)
    private void FloatInputOnly(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !FloatOnlyRegex().IsMatch(e.Text);
    }

    private void NumericPasteOnly(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!NumericOnlyRegex().IsMatch(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void FloatPasteOnly(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!FloatOnlyRegex().IsMatch(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }


    private void RunScriptButton_Click(object sender, RoutedEventArgs e)
    {
        string dataDir = DataDirTextBox.Text;
        string videoDir = VideoDirTextBox.Text;
        string exportDir = ExportDirTextBox.Text;

        // Other inputs omitted for brevity

        string arguments = $@"--data_dir {dataDir} --video_dir {videoDir} --export_dir {exportDir}";

        _pythonEnvironment.RunScript(arguments).Wait();
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

    [System.Text.RegularExpressions.GeneratedRegex("^[0-9]*(\\.[0-9]*)?$")]
    private static partial System.Text.RegularExpressions.Regex FloatOnlyRegex();

    [System.Text.RegularExpressions.GeneratedRegex("^[0-9]+$")]
    private static partial System.Text.RegularExpressions.Regex NumericOnlyRegex();

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl tabControl)
        {
            IsConsoleTabActive = tabControl.SelectedItem is TabItem tabItem &&
                                 tabItem.Header.ToString() != "Console";
        }
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}