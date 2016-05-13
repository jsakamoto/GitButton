using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GitButtons
{
    static class Program
    {
        private static System.Timers.Timer PollingTimer { get; } = new System.Timers.Timer { AutoReset = true, Interval = 400 };

        private static TGitWndStatus CurrentTGitWndStatus { get; set; } = new TGitWndStatus();

        private static event EventHandler<TGitWndStatusChangedEventArgs> TGitWndStatusChanged;

        private static SoundPlayer SoundPlayer { get; } = new SoundPlayer();

        private static SerialPort SerialPort { get; } = new SerialPort(AppSettings.Port);

        private static string _LineBuff = "";

        private static event EventHandler<SerialLineReceivedEventArgs> LineReceived;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();
            LineReceived += Program_LineReceived;

            SerialPort.DataReceived += SerialPort_DataReceived;
            SerialPort.Open();

            TGitWndStatusChanged += Program_TGitWndStatusChanged;

            PollingTimer.Elapsed += PollingTimer_Elapsed;
            PollingTimer.Start();

            Application.Run(mainForm);

            TGitWndStatusChanged -= Program_TGitWndStatusChanged;
            PollingTimer.Elapsed -= PollingTimer_Elapsed;
            PollingTimer.Dispose();

            SerialPort.DataReceived -= SerialPort_DataReceived;

            LineReceived -= Program_LineReceived;

            SerialPort.Dispose();
            SoundPlayer.Dispose();
            PollingTimer.Dispose();
        }

        private static void PollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var wndTitles = new List<string>();
            Win32API.EnumWindows((IntPtr hWnd, IntPtr lparam) =>
            {
                var wndTextLen = Win32API.GetWindowTextLength(hWnd);
                if (0 < wndTextLen)
                {
                    var wndText = new StringBuilder(wndTextLen + 1);
                    Win32API.GetWindowText(hWnd, wndText, wndText.Capacity);
                    wndTitles.Add(wndText.ToString());
                }
                return true;
            }, IntPtr.Zero);

            var latestTgitStatus = new TGitWndStatus
            {
                ExistsCommitWnd = wndTitles.Any(title => Regex.IsMatch(title, @" - Commit - TortoiseGit$")),
                ExistsProgressWnd = wndTitles.Any(title => Regex.IsMatch(title, @" - Git Command Progress - TortoiseGit$"))
            };

            var statusChanged = !latestTgitStatus.Equals(CurrentTGitWndStatus);
            if (statusChanged)
            {
                TGitWndStatusChanged?.Invoke(null, new TGitWndStatusChangedEventArgs(CurrentTGitWndStatus, latestTgitStatus));
                CurrentTGitWndStatus = latestTgitStatus;
            }
        }

        private static void Program_TGitWndStatusChanged(object sender, TGitWndStatusChangedEventArgs e)
        {
            // When opened commit window:
            if (e.PrevStatus.ExistsCommitWnd == false && e.CurrentStatus.ExistsCommitWnd == true)
            {
                // Play loop commiting Sound.
                var committngSound = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "committing.wav");
                if (File.Exists(committngSound))
                {
                    SoundPlayer.Stop();
                    SoundPlayer.SoundLocation = committngSound;
                    SoundPlayer.PlayLooping();
                }

                // Send begin-blink command to GitButton device.
                SerialPort.WriteLine("BB");
            }

            // When committed:
            else if (e.CurrentStatus.ExistsCommitWnd && e.CurrentStatus.ExistsProgressWnd)
            {
                // Stop loop commiting Sound & Play one shot commited Sound.
                var committedSound = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "committed.wav");
                if (File.Exists(committedSound))
                {
                    SoundPlayer.Stop();
                    SoundPlayer.SoundLocation = committedSound;
                    SoundPlayer.Play();
                }

                // Send end-blink command to GitButton device.
                SerialPort.WriteLine("EB");
            }

            // When closed commit window:
            else if (e.CurrentStatus.ExistsCommitWnd == false)
            {
                // Stop committing sound if it playing.
                if (Path.GetFileName(SoundPlayer.SoundLocation) == "committing.wav") SoundPlayer.Stop();

                // Send end-blink command to GitButton device.
                SerialPort.WriteLine("EB");
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

                LineReceived?.Invoke(serialPort, new SerialLineReceivedEventArgs(line));
            }
        }

        private static void Program_LineReceived(object sender, SerialLineReceivedEventArgs e)
        {
            switch (e.Line)
            {
                case "GIT COMMIT":
                    if (CurrentTGitWndStatus.ExistsCommitWnd && !CurrentTGitWndStatus.ExistsProgressWnd)
                        SendKeysToTGitCommitWnd("%o");
                    else if (CurrentTGitWndStatus.ExistsCommitWnd && CurrentTGitWndStatus.ExistsProgressWnd)
                        SendKeysToTGitProgressWnd("{ESC}");
                    else
                        SendKeysToVisualStudio("^g^c");
                    break;
                case "GIT PUSH":
                    SendKeysToVisualStudio("^g^p");
                    break;
                default:
                    break;
            }
        }

        private static void SendKeysToTGitCommitWnd(string keys)
        {
            SendKeysToApp(" - Commit - TortoiseGit", keys);
        }

        private static void SendKeysToTGitProgressWnd(string keys)
        {
            SendKeysToApp(" - Git Command Progress - TortoiseGit", keys);
        }

        private static void SendKeysToVisualStudio(string keys)
        {
            SendKeysToApp(" - Microsoft Visual Studio", keys);
        }

        private static void SendKeysToApp(string partialAppTitle, string keys)
        {
            var shell = new IWshRuntimeLibrary.WshShellClass();
            var app = (object)partialAppTitle;
            var success = shell.AppActivate(ref app);
            if (success)
            {
                shell.SendKeys(keys);
            }

            Marshal.ReleaseComObject(shell);
        }
    }
}

