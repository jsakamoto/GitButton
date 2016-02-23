using System;

namespace GitButtons
{
    internal class SerialLineReceivedEventArgs : EventArgs
    {
        public string Line { get; private set; }

        public SerialLineReceivedEventArgs(string line)
        {
            this.Line = line;
        }
    }
}