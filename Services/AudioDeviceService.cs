using NAudio.CoreAudioApi;
using phonic.Models;
using System.Runtime.InteropServices;

namespace phonic.Services;

public static class AudioDeviceService
{
    public static List<AudioDeviceItem> GetOutputDevices()
    {
        using var enumerator = new MMDeviceEnumerator();

        string? defaultId = null;
        try
        {
            using var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            defaultId = defaultDevice.ID;
        }
        catch { }

        var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        var devices = new List<AudioDeviceItem>(collection.Count);

        for (var i = 0; i < collection.Count; i++)
        {
            using var d = collection[i];
            devices.Add(new AudioDeviceItem
            {
                Id = d.ID,
                Name = d.FriendlyName,
                IsDefault = d.ID == defaultId
            });
        }

        return devices;
    }

    public static float GetMasterVolume()
    {
        using var enumerator = new MMDeviceEnumerator();
        try
        {
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.AudioEndpointVolume.MasterVolumeLevelScalar;
        }
        catch { return 0f; }
    }

    public static void SetMasterVolume(float volume)
    {
        using var enumerator = new MMDeviceEnumerator();
        try
        {
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volume, 0f, 1f);
        }
        catch { }
    }

    public static bool SetDefaultDevice(string deviceId)
    {
        try
        {
            var config = (IPolicyConfig)Activator.CreateInstance(
                Type.GetTypeFromCLSID(new Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9"))!)!;

            var hr = config.SetDefaultEndpoint(deviceId, Role.Multimedia);
            if (hr != 0) return false;

            config.SetDefaultEndpoint(deviceId, Role.Communications);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

[ComImport]
[Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IPolicyConfig
{
    [PreserveSig] int GetMixFormat(string pszDeviceName, IntPtr ppFormat);
    [PreserveSig] int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);
    [PreserveSig] int ResetDeviceFormat(string pszDeviceName);
    [PreserveSig] int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr pMixFormat);
    [PreserveSig] int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);
    [PreserveSig] int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);
    [PreserveSig] int GetShareMode(string pszDeviceName, IntPtr pMode);
    [PreserveSig] int SetShareMode(string pszDeviceName, IntPtr pMode);
    [PreserveSig] int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr pKey, IntPtr pv);
    [PreserveSig] int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr pKey, IntPtr pv);
    [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, Role role);
    [PreserveSig] int SetEndpointVisibility(string pszDeviceName, bool bVisible);
}
