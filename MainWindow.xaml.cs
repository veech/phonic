using phonic.Models;
using phonic.Services;
using System.Windows;
using System.Windows.Controls.Primitives;
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
    DateTime _volRepeatTime = DateTime.MinValue;
    bool _suppressVolumeChange;

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
        LoadVolume();
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

    void LoadVolume()
    {
        _suppressVolumeChange = true;
        var volume = (int)Math.Round(AudioDeviceService.GetMasterVolume() * 100);
        VolumeSlider.Value = volume;
        VolumeLabel.Text = $"{volume}%";
        _suppressVolumeChange = false;
    }

    void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressVolumeChange) return;
        var pct = (int)e.NewValue;
        VolumeLabel.Text = $"{pct}%";
        AudioDeviceService.SetMasterVolume(pct / 100f);
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
            case Key.Left:
                AdjustVolume(-5);
                e.Handled = true;
                break;
            case Key.Right:
                AdjustVolume(5);
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

        HandleNavRepeat(state, prev);
        HandleVolumeRepeat(state, prev);
    }

    void HandleNavRepeat(XInputService.ControllerState state, XInputService.ControllerState? prev)
    {
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

    void HandleVolumeRepeat(XInputService.ControllerState state, XInputService.ControllerState? prev)
    {
        var left = state.DpadLeft || state.StickLeft;
        var right = state.DpadRight || state.StickRight;
        var prevLeft = (prev?.DpadLeft ?? false) || (prev?.StickLeft ?? false);
        var prevRight = (prev?.DpadRight ?? false) || (prev?.StickRight ?? false);

        if (left && !prevLeft) { AdjustVolume(-5); _volRepeatTime = DateTime.Now.AddMilliseconds(400); }
        else if (right && !prevRight) { AdjustVolume(5); _volRepeatTime = DateTime.Now.AddMilliseconds(400); }
        else if ((left || right) && DateTime.Now >= _volRepeatTime)
        {
            AdjustVolume(left ? -5 : 5);
            _volRepeatTime = DateTime.Now.AddMilliseconds(100);
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

    void AdjustVolume(int delta)
    {
        VolumeSlider.Value = Math.Clamp(VolumeSlider.Value + delta, 0, 100);
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
        LoadVolume();
    }

    void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
        StatusText.Visibility = Visibility.Visible;
    }
}
