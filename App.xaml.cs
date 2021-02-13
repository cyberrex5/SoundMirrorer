using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SoundMirrorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool IsMirroring { get; private set; } = false;
        public static Dictionary<string, MMDevice> DevicesMap = new Dictionary<string, MMDevice>();
        public static string SourceDevice
        {
            get
            {
                if (_sourceDevice != null)
                {
                    return _sourceDevice.FriendlyName;
                }
                return null;
            }
            set
            {
                if (_sourceDevice != null && value == _sourceDevice.FriendlyName)
                {
                    Debug.WriteLine($"\n{_sourceDevice.FriendlyName} is already the source device");
                    return;
                }

                Application.Current.MainWindow.IsEnabled = false;
                if (IsMirroring)
                {
                    StopMirroring();
                    SetSourceDevice();
                    StartMirroring();
                }
                else
                {
                    SetSourceDevice();
                }
                Application.Current.MainWindow.IsEnabled = true;

                void SetSourceDevice()
                {
                    if (DevicesMap.TryGetValue(value, out _sourceDevice))
                        Debug.WriteLine($"\nChanged source device to {_sourceDevice.FriendlyName}\n");
                }
            }
        }
        public static void AddOutputDevice(string name)
        {
            Application.Current.MainWindow.IsEnabled = false;
            if (IsMirroring)
            {
                StopMirroring();
                AddDevice();
                StartMirroring();
            }
            else
            {
                AddDevice();
            }
            Application.Current.MainWindow.IsEnabled = true;

            void AddDevice()
            {
                _outputDevices.Add(name, DevicesMap[name]);
                Debug.WriteLine($"\nAdded {name} to output devices\n");
            }
        }
        public static void RemoveOutputDevice(string device)
        {
            Application.Current.MainWindow.IsEnabled = false;
            if (IsMirroring)
            {
                StopMirroring();
                if (_outputDevices.ContainsKey(device))
                {
                    _outputDevices.Remove(device);
                }
                else
                {
                    Debug.WriteLine($"\n{device} not in App.{nameof(_outputDevices)}\n");
                }
                StartMirroring();
            }
            else
            {
                if (_outputDevices.ContainsKey(device))
                {
                    _outputDevices.Remove(device);
                }
                else
                {
                    Debug.WriteLine($"\n{device} not in App.{nameof(_outputDevices)}\n");
                }
            }
            Application.Current.MainWindow.IsEnabled = true;
        }
        public static int OutputDevicesCount => _outputDevices.Count;
        public static bool IsOutputDevice(string deviceName) => _outputDevices.ContainsKey(deviceName);

        private static MMDevice _sourceDevice = null;
        private static Dictionary<string, MMDevice> _outputDevices = new Dictionary<string, MMDevice>();

        private static WasapiLoopbackCapture capture = null;
        private static List<MirrorHandler> mirrors = new List<MirrorHandler>();

        private static TaskbarIcon ti;


        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            ti = (TaskbarIcon)Application.Current.FindResource("TaskbarIco");

            MMDeviceCollection playbackDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in playbackDevices)
            {
                DevicesMap.Add(device.FriendlyName, device);
            }
        }

        public static void StartMirroring()
        {
            Application.Current.MainWindow.IsEnabled = false;

            capture = new WasapiLoopbackCapture(_sourceDevice);
            foreach (MMDevice outputDevice in _outputDevices.Values)
            {
                mirrors.Add(new MirrorHandler(ref capture, outputDevice));
            }

            Debug.WriteLine($"\nAudio source device: {_sourceDevice.FriendlyName}");

            MirrorHandler[] mirrorsArr = mirrors.ToArray();
            for (int i = 0; i < mirrorsArr.Length; ++i)
            {
                mirrorsArr[i].StartMirroring();
            }
            capture.StartRecording();

            IsMirroring = true;
            ti.ToolTipText = $"SoundMirrorer (Mirroring from {_sourceDevice.FriendlyName})";

            Application.Current.MainWindow.IsEnabled = true;
        }

        public static void StopMirroring()
        {
            Application.Current.MainWindow.IsEnabled = false;

            capture.StopRecording();
            foreach (MirrorHandler mirror in mirrors)
            {
                mirror.StopMirroring();
            }
            mirrors.Clear();
            capture.Dispose();

            capture = null;

            IsMirroring = false;
            ti.ToolTipText = "SoundMirrorer (Not Mirroring)";

            Application.Current.MainWindow.IsEnabled = true;
        }

        private void tbShow_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.Visibility == Visibility.Visible)
            {
                return;
            }
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
        }
        private void tbExit_Click(object sender, RoutedEventArgs e)
        {
            if (IsMirroring)
            {
                StopMirroring();
            }
            Application.Current.Shutdown();
        }
    }
}
