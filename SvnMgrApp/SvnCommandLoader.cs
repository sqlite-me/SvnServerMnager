//using SuperSocket.Common;
//using SuperSocket.SocketBase;
//using SuperSocket.SocketBase.Command;
//using SuperSocket.SocketBase.Config;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SvnMgrApp
//{
//    class SvnCommandLoader : ICommandLoader<CommandBase<SvnMgrSession, JsonCmdPackageInfo>>
//    {
//        public event EventHandler<CommandUpdateEventArgs<CommandBase<SvnMgrSession, JsonCmdPackageInfo>>> Updated;
//        public event EventHandler<ErrorEventArgs> Error;

//        public bool Initialize(IRootConfig rootConfig, IAppServer appServer)
//        {
//            appServer
//        }

//        public bool TryLoadCommands(out IEnumerable<CommandBase<SvnMgrSession, JsonCmdPackageInfo>> commands)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
