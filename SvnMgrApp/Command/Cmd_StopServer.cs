using SuperSocket.SocketBase.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp.Command
{
   public class Cmd_StopServer : CommandBase<SvnMgrSession, JsonCmdPackageInfo>
    {
        public override string Name => "QUIT";
        public override void ExecuteCommand(SvnMgrSession session, JsonCmdPackageInfo requestInfo)
        {
            if (session.Logined)
            {
                Program.Quit = true;
                session.Success(requestInfo);
                session.AppServer.Stop();
            }
            else
            {
                requestInfo.Send(session);
            }
        }
    }
}
