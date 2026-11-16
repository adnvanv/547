using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StayPlanner.Business;
using StayPlanner.Data;
using StayPlanner.Desktop.ViewModels;
using StayPlanner.Desktop.Views;

namespace StayPlanner.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Wire the layers together: client -> business -> data.
            var rooms = new EmbeddedRoomRepository();
            var reservations = new JsonReservationRepository(Environment.GetEnvironmentVariable("STAYPLANNER_DATA"));
            var quotes = new QuoteManager(rooms);
            var manager = new ReservationManager(rooms, reservations, quotes);

            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(quotes, manager) };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
