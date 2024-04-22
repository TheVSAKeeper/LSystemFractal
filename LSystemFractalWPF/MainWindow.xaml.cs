using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LSystemFractalWPF;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string Axiom = "22220";

    private readonly Dictionary<char, string> _rules;
    private readonly Stack<int> _stack;

    public MainWindow()
    {
        _rules = new Dictionary<char, string>
        {
            { '1', "21" },
            { '0', "1[-20]+20" }
        };

        _stack = new Stack<int>();

        Iterations = 12;
        Angle = 16;
        Distance = 10;

        InitializeComponent();
        DataContext = this;
    }

    public int Iterations { get; set; }

    public double Angle { get; set; }

    public double Distance { get; set; }

    private void OnRedrawButtonClicked(object sender, RoutedEventArgs e)
    {
        ProgressBar.Value = 0;
        _stack.Clear();
        Canvas.Children.Clear();
        DrawFractal();
    }

    private void DrawFractal()
    {
        StringBuilder axiomBuilder = new(Axiom);
        int totalIterations = Iterations * Axiom.Length;
        int currentIteration = 0;

        for (int i = 0; i < Iterations; i++)
        {
            StringBuilder tempAxiomBuilder = new();

            foreach (char ch in axiomBuilder.ToString())
            {
                if (_rules.TryGetValue(ch, out string? value))
                    tempAxiomBuilder.Append(value);
                else
                    tempAxiomBuilder.Append(ch);

                currentIteration++;
                ReportProgress(currentIteration, totalIterations);
            }

            axiomBuilder = tempAxiomBuilder;
        }

        DrawLSystem(axiomBuilder.ToString());
    }

    private void ReportProgress(int currentIteration, int totalIterations)
    {
        double progress = (double)currentIteration / totalIterations * 100;

        ProgressBar.Dispatcher.Invoke(() =>
        {
            ProgressBar.Value = progress;
        });
    }

    private void DrawLSystem(string instructions)
    {
        double x = Canvas.ActualWidth / 2;
        double y = Canvas.ActualHeight - 10;
        double angle = -90;
        double radians = angle * Math.PI / 180;
        SolidColorBrush brush = Brushes.Black;
        double thickness = 1;

        foreach (char instruction in instructions)
        {
            Line line;
            double x1;
            double y1;

            switch (instruction)
            {
                case '0':
                    _stack.Push((int)thickness);
                    thickness = 4;

                    int randomValue = new Random().Next(0, 11);

                    brush = new SolidColorBrush(randomValue switch
                    {
                        < 3 => Color.FromRgb(0, 153, 0),
                        > 6 => Color.FromRgb(102, 121, 0),
                        var _ => Color.FromRgb(32, 187, 0)
                    });

                    x1 = x + Distance / 2f * Math.Cos(radians);
                    y1 = y + Distance / 2f * Math.Sin(radians);

                    line = new Line
                    {
                        X1 = x,
                        Y1 = y,
                        X2 = x1,
                        Y2 = y1,
                        Stroke = brush,
                        StrokeThickness = thickness
                    };

                    Canvas.Children.Add(line);

                    thickness = _stack.Pop();
                    brush = Brushes.Black;
                    break;

                case '1':
                case '2':
                    x1 = x + Distance * Math.Cos(radians);
                    y1 = y + Distance * Math.Sin(radians);

                    line = new Line
                    {
                        X1 = x,
                        Y1 = y,
                        X2 = x1,
                        Y2 = y1,
                        Stroke = brush,
                        StrokeThickness = thickness
                    };

                    Canvas.Children.Add(line);

                    x = x1;
                    y = y1;
                    break;

                case '+':
                    angle += Angle;
                    radians = angle * Math.PI / 180;
                    break;

                case '-':
                    angle -= Angle;
                    radians = angle * Math.PI / 180;
                    break;

                case '[':
                    _stack.Push((int)x);
                    _stack.Push((int)y);
                    _stack.Push((int)angle);
                    _stack.Push((int)thickness);
                    break;

                case ']':
                    thickness = _stack.Pop();
                    angle = _stack.Pop();
                    y = _stack.Pop();
                    x = _stack.Pop();
                    radians = angle * Math.PI / 180;
                    break;
            }
        }
    }
}