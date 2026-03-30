using phonic.Models;
using phonic.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace phonic;

public partial class MainWindow : Window
{
    List<AudioDeviceItem> _devices = [];
    readonly DispatcherTimer _controllerTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    XInputService.ControllerState? _prevControllerState;
    DateTime _navRepeatTime = DateTime.MinValue;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += (_, _) => _controllerTimer.Stop();
        PreviewKeyDown += OnPreviewKeyDown;
        _controllerTimer.Tick += OnControllerTick;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadDevices();
        Keyboard.Focus(DeviceList);
        _controllerTimer.Start();
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

    void OnControllerTick(object? sender, EventArgs e)
    {
        var state = XInputService.GetStateForAnyController();
        if (state == null)
        {
            _prevControllerState = null;
            return;
        }

        var prev = _prevControllerState;
        _prevControllerState = state;

        if (state.A && !(prev?.A ?? false)) { SelectDevice(); return; }
        if ((state.B || state.Back) && !(prev?.B ?? false) && !(prev?.Back ?? false))
        {
            Application.Current.Shutdown();
            return;
        }

        var up = state.DpadUp || state.StickUp;
        var down = state.DpadDown || state.StickDown;
        var prevUp = (prev?.DpadUp ?? false) || (prev?.StickUp ?? false);
        var prevDown = (prev?.DpadDown ?? false) || (prev?.StickDown ?? false);

        if (up && !prevUp) { NavigateUp(); _navRepeatTime = DateTime.Now.AddMilliseconds(400); }
        else if (down && !prevDown) { NavigateDown(); _navRepeatTime = DateTime.Now.AddMilliseconds(400); }
        else if ((up || down) && DateTime.Now >= _navRepeatTime)
        {
            if (up) NavigateUp();
            else NavigateDown();
            _navRepeatTime = DateTime.Now.AddMilliseconds(150);
        }
    }

    void NavigateUp()
    {
        if (DeviceList.SelectedIndex <= 0) return;
        DeviceList.SelectedIndex--;
        DeviceList.ScrollIntoView(DeviceList.SelectedItem);
    }

    void NavigateDown()
    {
        if (DeviceList.SelectedIndex >= _devices.Count - 1) return;
        DeviceList.SelectedIndex++;
        DeviceList.ScrollIntoView(DeviceList.SelectedItem);
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

        ShowSuccess(device.Name);
    }

    void ShowSuccess(string deviceName)
    {
        StatusText.Text = $"Switched to {deviceName}";
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
        StatusText.Visibility = Visibility.Visible;
        LoadDevices();
    }

    void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
        StatusText.Visibility = Visibility.Visible;
    }
}
