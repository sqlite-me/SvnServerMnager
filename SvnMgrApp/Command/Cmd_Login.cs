using SuperSocket.SocketBase.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp.Command
{
  public  class Cmd_Login : CommandBase<SvnMgrSession, JsonCmdPackageInfo>
    {
        public override string Name => "LOGIN";
        public override void ExecuteCommand(SvnMgrSession session, JsonCmdPackageInfo requestInfo)
        {
            if (requestInfo.Body == SvnMgrClient.MgrClient.SSKey)
            {
                session.SSKey = requestInfo.Body;
                session.Success(requestInfo);
            }
            else
            {
                requestInfo.Send(session);
            }
        }
    }
}
