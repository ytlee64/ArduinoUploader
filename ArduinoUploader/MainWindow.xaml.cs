using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Enumeration;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ArduinoUploader
{
    public partial class MainWindow : Window
    {
        public static string OutputText="";
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();

            ProgrammerName.Text = IniData.iniGet("Arduino", "ProgrammerName", "arduino");
            ProcessName.Text= IniData.iniGet("Arduino", "ProcessName", "atmega328p");
            Baud.Text = IniData.iniGet("Arduino", "Baud", "115200");
            Filename.Text = IniData.iniGet("Arduino", "Filename", "blink.txt");

            //IniData.iniSet("Arduino", "ProgrammerName", ProgrammerName.Text);
            //IniData.iniSet("Arduino", "ProcessName", ProcessName.Text);
            //IniData.iniSet("Arduino", "Baud", Baud.Text);
            //IniData.iniSet("Arduino", "Filename", Filename.Text);


            LoadSerialPorts();
        }
        private void LoadSerialPorts()
        {
            // 현재 시스템의 모든 시리얼 포트 이름을 가져옵니다.
            string[] ports = SerialPort.GetPortNames();

            // ComboBox를 초기화하고 시리얼 포트를 추가합니다.
            SerialPortComboBox.Items.Clear();
            foreach (string port in ports)
            {
                SerialPortComboBox.Items.Add(port);
            }

            // 포트가 하나 이상 있는 경우 첫 번째 포트를 기본 선택합니다.
            if (SerialPortComboBox.Items.Count > 0)
            {
                SerialPortComboBox.SelectedIndex = 0;
            }
        }

        private void RunAvrdude_Click(object sender, RoutedEventArgs e)
        {
            string ?port=null;

            if (SerialPortComboBox.SelectedItem != null)
            {
                // 선택된 콤보박스 항목을 가져옵니다.
                port = SerialPortComboBox.SelectedItem.ToString();
                //MessageBox.Show($"Selected Port: {port}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (string.IsNullOrWhiteSpace(port))
            {
                MessageBox.Show("포트를 확인하세요.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
          
            // AVRDUDE 명령어 예시
            string arguments = $"-v -p{ProcessName.Text} -c{ProgrammerName.Text} -P{port} -b{Baud.Text} -D -Uflash:w:{Filename.Text}:i";
            // avrdude -c {board} -p m328p -P COM7 -b 115200 -U flash:w:blink.hex:i
            // avrdude -v -patmega328p -carduino -PCOM7 -b115200 -D -Uflash:w:Blink.hex:i

            // AVRDUDE 실행
            
            Task task = Task.Run(() => RunAvrdude(arguments));

            // Task가 완료되길 기다림
            //task.Wait();
        }


        private void RunAvrdude(string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "avrdude.exe", // AVRDUDE 실행 파일 경로
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                process.OutputDataReceived += (sender, args) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                            WeakReferenceMessenger.Default.Send((string)args.Data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                };

                
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        WeakReferenceMessenger.Default.Send((string)args.Data);
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                
                Console.WriteLine($"process.WaitForExit()");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run AVRDUDE: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OutputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputTextBox.ScrollToEnd();
        }
    }
}
