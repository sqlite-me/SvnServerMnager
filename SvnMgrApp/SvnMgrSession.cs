using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{
   public class SvnMgrSession : AppSession<SvnMgrSession, JsonCmdPackageInfo>
    {
        public SvnMgrSession() {
        
        }
        public string SSKey { get; internal set; }

        public bool Logined => SSKey != null;

        public void Success(JsonCmdPackageInfo requestInfo)
        {
            new JsonCmdPackageInfo(requestInfo.Cmd,"success", requestInfo.Id).Send(this);
        }

        public void Error(JsonCmdPackageInfo requestInfo)
        {
            new JsonCmdPackageInfo(requestInfo.Cmd, "error", requestInfo.Id).Send(this);
        }
    }
}
