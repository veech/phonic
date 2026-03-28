using phonic.Models;
using phonic.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace phonic;

public partial class MainWindow : Window
{
    List<AudioDeviceItem> _devices = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadDevices();
        Keyboard.Focus(DeviceList);
    }

    void LoadDevices()
    {
        _devices = AudioDeviceService.GetOutputDevices();

        if (_devices.Count == 0)
        {
            ShowError("No audio output devices found.");
            return;
        }

        DeviceList.ItemsSource = _devices;

        var defaultIndex = _devices.FindIndex(d => d.IsDefault);
        DeviceList.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        DeviceList.ScrollIntoView(DeviceList.SelectedItem);
    }

    void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
                SelectDevice();
                e.Handled = true;
                break;
            case Key.Escape:
            case Key.Back:
                Application.Current.Shutdown();
                e.Handled = true;
                break;
        }
    }

    void SelectDevice()
    {
        if (DeviceList.SelectedItem is not AudioDeviceItem device) return;

        var success = AudioDeviceService.SetDefaultDevice(device.Id);

        if (!success)
        {
            ShowError($"Failed to switch to {device.Name}.");
            return;
        }

        ExitWithSuccess(device.Name);
    }

    async void ExitWithSuccess(string deviceName)
    {
        StatusText.Text = $"Switched to {deviceName}";
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
        StatusText.Visibility = Visibility.Visible;

        await Task.Delay(900);
        Application.Current.Shutdown();
    }

    void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
        StatusText.Visibility = Visibility.Visible;
    }
}
