using ExampleInSimApp.Properties;
using InSimDotNet;
using InSimDotNet.Helpers;
using System;
using System.Windows.Forms;

namespace ExampleInSimApp {
    public partial class MainForm : Form {
        private RaceControl raceControl;

        public MainForm() {
            InitializeComponent();

            // load settings
            hostAddressTextBox.Text = Settings.Default.HostAddress;
            insimPortNumericUpDown.Value = Settings.Default.InSimPort;
            adminPassTextBox.Text = Settings.Default.AdminPass;

            // the RaceControl object represents the connection with LFS
            raceControl = new RaceControl();
            raceControl.Initialized += raceControl_Initialized;
            raceControl.Disconnected += raceControl_Disconnected;
            raceControl.InSimError += raceControl_InSimError;
            raceControl.LapCompleted += raceControl_LapCompleted;

            UpdateUI();

            ActiveControl = connectButton;
        }

        private void UpdateUI() {
            // update the UI depending on whether we are connected or not.
            connectButton.Enabled = !raceControl.IsConnected;
            disconnectButton.Enabled = raceControl.IsConnected;
            hostAddressTextBox.Enabled = !raceControl.IsConnected;
            insimPortNumericUpDown.Enabled = !raceControl.IsConnected;
            adminPassTextBox.Enabled = !raceControl.IsConnected;

            // update form title
            Text = "LapTimes";
            if (raceControl.IsConnected) {
                Text += " [Connected]";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            // program closing so save the settings.
            Settings.Default.HostAddress = hostAddressTextBox.Text;
            Settings.Default.InSimPort = (int)insimPortNumericUpDown.Value;
            Settings.Default.AdminPass = adminPassTextBox.Text;
            Settings.Default.Save();
        }

        private void raceControl_Initialized(object sender, InitializeEventArgs e) {
            // called when LFS is connected
            UpdateUI();
        }

        private void raceControl_Disconnected(object sender, DisconnectedEventArgs e) {
            // called when LFS disconnects
            UpdateUI();
        }

        private void raceControl_InSimError(object sender, InSimErrorEventArgs e) {
            // called if an error occurs on a background thread
            UpdateUI();

            MessageBox.Show(this, e.Exception.Message, "InSim Error");
        }

        private void raceControl_LapCompleted(object sender, LapCompletedEventArgs e) {
            // add new item to listview
            var item = new ListViewItem();
            item.Text = e.PlayerName;
            item.SubItems.Add(e.Car);
            item.SubItems.Add(e.Track);
            item.SubItems.Add(e.Laps.ToString());
            item.SubItems.Add(GetTimeString(e.LapTime));
            item.SubItems.Add(GetTimeString(e.Splits[0]));
            item.SubItems.Add(GetTimeString(e.Splits[1]));
            item.SubItems.Add(GetTimeString(e.Splits[2]));
            lapsListView.Items.Add(item);
        }

        private string GetTimeString(TimeSpan time) {
            // gets the nicely formatted time string or empty if there is no time
            if (time == TimeSpan.Zero) {
                return String.Empty;
            }

            return StringHelper.ToLapTimeString(time);
        }

        private void connectButton_Click(object sender, EventArgs e) {
            // connect to LFS
            try {
                Cursor = Cursors.WaitCursor;

                raceControl.Initialize(
                    hostAddressTextBox.Text,
                    (int)insimPortNumericUpDown.Value,
                    adminPassTextBox.Text);
            }
            catch(Exception ex) {
                MessageBox.Show(this, ex.Message, "InSim Error");
            }
            finally {
                Cursor = null; // reset cursor
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e) {
            try {
                raceControl.Disconnect();
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message, "InSim Error");
            }
        }
    }
}
