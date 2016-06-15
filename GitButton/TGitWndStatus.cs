using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitButton
{
    public struct TGitWndStatus
    {
        public bool ExistsCommitWnd { get; set; }

        public bool ExistsProgressWnd { get; set; }

        public override string ToString() => $"ExistsCommitWnd: {ExistsCommitWnd}, ExistsProgressWnd: {ExistsProgressWnd}";
    }
}
