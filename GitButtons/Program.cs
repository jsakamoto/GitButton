using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitButtons
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();
            _LineReceived += mainForm.Program_LineReceived;

            var breakEvent = new ManualResetEvent(initialState: false);
            var comloopTask = Task.Run(() => SerialComLoop(breakEvent));

            Application.Run(mainForm);

            _LineReceived -= mainForm.Program_LineReceived;
            breakEvent.Set();
            comloopTask.Wait();
        }

        private static string _LineBuff = "";

        private static event EventHandler<SerialLineReceivedEventArgs> _LineReceived;

        private static void SerialComLoop(ManualResetEvent breakEvent)
        {
            using (var serialPort = new SerialPort("COM3"))
            {
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;

                breakEvent.WaitOne();
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = sender as SerialPort;
            var bytes = new byte[serialPort.BytesToRead];
            serialPort.Read(bytes, 0, bytes.Length);
            _LineBuff += Encoding.ASCII.GetString(bytes);

            var n = default(int);
            while ((n = _LineBuff.IndexOf('\n')) != -1)
            {
                var line = _LineBuff.Substring(0, n).TrimEnd('\r');
                _LineBuff = _LineBuff.Substring(n + 1);

                _LineReceived?.Invoke(serialPort, new SerialLineReceivedEventArgs(line));
            }
        }
    }
}
