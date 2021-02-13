using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;

namespace SoundMirrorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ListPlaybackDevices();
            EnabledCheckBox.IsChecked = App.IsMirroring;
        }

        #region Event Handlers

        private void Enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (App.IsMirroring) return;

            if (App.SourceDevice == null || App.OutputDevicesCount == 0)
            {
                EnabledCheckBox.IsChecked = false;
                MessageBox.Show("You have to select a Device to mirror sound from,\nand a Device to mirror sound to", "No Device Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App.StartMirroring();
        }
        private void Enabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!App.IsMirroring) return;

            if (App.SourceDevice != null || App.OutputDevicesCount == 0)
            {
                App.StopMirroring();
            }
        }

        private void SourceDevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SourceDevicesComboBox.SelectedIndex == -1) return;

            if (App.IsOutputDevice(SourceDevicesComboBox.SelectedItem.ToString()))
            {
                SourceDevicesComboBox.SelectedIndex = -1;
                MessageBox.Show("Can't mirror sound to same device", "Device already selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App.SourceDevice = SourceDevicesComboBox.SelectedItem.ToString();
        }
        private void OutputDevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputDevicesComboBox.SelectedIndex == -1) return;

            Debug.WriteLine($"\n{nameof(OutputDevicesComboBox)} opened\n");

            OutputDevicesComboBox.Visibility = Visibility.Hidden;

            if (SourceDevicesComboBox.SelectedIndex != -1 && OutputDevicesComboBox.SelectedItem.ToString() == SourceDevicesComboBox.SelectedItem.ToString())
            {
                MessageBox.Show("Can't mirror sound to same device", "Device already selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OutputDevicesList.Items.Add(OutputDevicesComboBox.SelectedItem);

            App.AddOutputDevice(OutputDevicesComboBox.SelectedItem.ToString());

            OutputDevicesComboBox.Items.Remove(OutputDevicesComboBox.SelectedItem);
        }
        private void OutputDevicesComboBox_MouseChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (OutputDevicesComboBox.IsDropDownOpen)
            {
                OutputDevicesComboBox.IsDropDownOpen = false;
            }
        }
        private void OutputDevicesComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            OutputDevicesComboBox.Visibility = Visibility.Hidden;
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            OutputDevicesComboBox.SelectedIndex = -1;
            OutputDevicesComboBox.Visibility = Visibility.Visible;
            OutputDevicesComboBox.IsDropDownOpen = true;
            Debug.WriteLine("\nAdd Device clicked\n");
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            if (OutputDevicesList.SelectedIndex == -1) return;

            if (OutputDevicesList.Items.Count == 1)
            {
                EnabledCheckBox.IsChecked = false; // should also stop mirroring
            }
            App.RemoveOutputDevice(OutputDevicesList.SelectedItem.ToString());
            OutputDevicesComboBox.Items.Add(OutputDevicesList.SelectedItem);
            OutputDevicesList.Items.Remove(OutputDevicesList.SelectedItem);
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show
            ("There will always be a slight delay for the devices your mirroring to.\nif it bothers you, you can change your playback device in windows to one you're not using and mirror that to the devices you want to use (this way, both the devices' sound will be slightly delayed)",
            "Audio Delay", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        private void ListPlaybackDevices()
        {
            foreach (string deviceName in App.DevicesMap.Keys)
            {
                SourceDevicesComboBox.Items.Add(deviceName);
                if (App.IsOutputDevice(deviceName))
                {
                    OutputDevicesList.Items.Add(deviceName);
                }
                else
                {
                    OutputDevicesComboBox.Items.Add(deviceName);
                }
            }
            SourceDevicesComboBox.SelectedItem = App.SourceDevice;
        }
    }
}
