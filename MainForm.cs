using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO.Ports;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace ESP32FirmwareUpgradeApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            PopulateCOMPortComboBox();
            LoadFirmwareFiles();
        }

        private void PopulateCOMPortComboBox()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBoxCOMPorts.Items.Clear();
            comboBoxCOMPorts.Items.AddRange(ports);
        }

        private void LoadFirmwareFiles()
        {
            string firmwareDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Firmware");
            if (!Directory.Exists(firmwareDirectory))
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select the Firmware Directory";
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        firmwareDirectory = folderBrowserDialog.SelectedPath;
                    }
                    else
                    {
                        MessageBox.Show("No firmware directory selected. Please select a firmware file.");
                        return;
                    }
                }
            }

            LoadFirmwareFilesFromDirectory(firmwareDirectory);
        }

        private void LoadFirmwareFilesFromDirectory(string firmwareDirectory)
        {
            string[] files = Directory.GetFiles(firmwareDirectory, "*.bin");
            cmbFirmwares.Items.Clear();
            foreach (string file in files)
            {
                cmbFirmwares.Items.Add(Path.GetFileName(file));
            }
        }


        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            string selectedPort = comboBoxCOMPorts.SelectedItem?.ToString() ?? "";
            string selectedFirmware = cmbFirmwares.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrEmpty(selectedPort))
            {
                MessageBox.Show("Please select a COM port.");
                return;
            }

            if (string.IsNullOrEmpty(selectedFirmware))
            {
                MessageBox.Show("Please select a firmware file.");
                return;
            }

            //string firmwareFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Firmware", selectedFirmware);
            //string firmwareFilePath= Path.Combine("Firmware", selectedFirmware);
            string firmwareFilePath = selectedFirmware;
            RunFirmwareUpgrade(selectedPort, firmwareFilePath);
        }

        private void WriteLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(WriteLog), new object[] { message });
            }
            else
            {
                txtLog.AppendText(message + Environment.NewLine);
                txtLog.ScrollToCaret(); // Scrolls the TextBox to the end
            }
        }

        private void RunFirmwareUpgrade(string comport, string firmwareFilePath)
        {
            string cwd = $"{Directory.GetCurrentDirectory()}";
            //string toolPath = @"esptool.exe"; // Update with the actual path
            string toolPath = cwd+@"\esptool.exe";

           
            
            string args = $"--chip esp32s2 --port \"{comport}\" --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 80m --flash_size detect 0x1000 \"{cwd}\\bootloader_dio_80m.bin\" 0x8000 \"{cwd}\\partitions.bin\" 0xe000 \"{cwd}\\boot_app0.bin\" 0x10000 \"{cwd}\\Firmware\\{firmwareFilePath}";

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "\""+toolPath+"\"",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                // Read and log the standard output of the process
                while (!process.StandardOutput.EndOfStream)
                {
                    string output = process.StandardOutput.ReadLine();
                    WriteLog(output);
                }

                // Read and log the standard error of the process (if any)
                while (!process.StandardError.EndOfStream)
                {
                    string error = process.StandardError.ReadLine();
                    WriteLog("Error: " + error);
                }

                process.WaitForExit(); // Optional: Wait for the process to exit
            }

            WriteLog("Upgrade completed.");
        }



        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void comboBoxCOMPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            WriteLog($"Port Selected '{this.comboBoxCOMPorts.Text}'");
        }

        private void cmbFirmwares_SelectedIndexChanged(object sender, EventArgs e)
        {
            WriteLog($"Firmware Selected '{this.cmbFirmwares.Text}'");
        }
    }
}
