using System;

namespace GitButtons
{
    public class TGitWndStatusChangedEventArgs : EventArgs
    {
        public TGitWndStatus PrevStatus { get; private set; }

        public TGitWndStatus CurrentStatus { get; private set; }

        public TGitWndStatusChangedEventArgs(TGitWndStatus prevStatus, TGitWndStatus currentStatus)
        {
            this.PrevStatus = prevStatus;
            this.CurrentStatus = currentStatus;
        }
    }
}