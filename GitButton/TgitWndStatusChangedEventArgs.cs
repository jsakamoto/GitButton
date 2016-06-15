using System;

namespace GitButton
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