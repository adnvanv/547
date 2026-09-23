using System.Windows;

namespace VetClinic.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FontSize = App.Settings.FontSize;
    }

    public void SetStatus(string message) => StatusText.Text = $"{DateTime.Now:t}  {message}";
}
