using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GitButtons
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        internal void Program_LineReceived(object sender, SerialLineReceivedEventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                switch (e.Line)
                {
                    case "GIT COMMIT":
                        SendKeysToVisualStudio("^g^c");
                        break;
                    case "GIT PUSH":
                        SendKeysToVisualStudio("^g^p");
                        break;
                    default:
                        break;
                }
            }));
        }

        private static void SendKeysToVisualStudio(string keys)
        {
            var shell = new IWshRuntimeLibrary.WshShellClass();
            var app = (object)" - Microsoft Visual Studio";
            var success = shell.AppActivate(ref app);
            if (success)
            {
                shell.SendKeys(keys);
            }

            Marshal.ReleaseComObject(shell);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
