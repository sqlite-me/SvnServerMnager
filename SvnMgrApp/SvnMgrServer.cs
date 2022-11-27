using NLog;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SvnMgrClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{
    class SvnMgrServer: AppServer<SvnMgrSession, JsonCmdPackageInfo>
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public SvnMgrServer() 
            : base(new DefaultReceiveFilterFactory<SvnMgrReceiveFilter, JsonCmdPackageInfo>())
        {
            //NewSessionConnected += new SessionHandler<SvnMgrSession>(appServer_NewSessionConnected);
            //NewRequestReceived += new RequestHandler<SvnMgrSession, StringRequestInfo>(appServer_NewRequestReceived);
        }


        private void appServer_NewSessionConnected(SvnMgrSession session)
        {
        }

        //void appServer_NewRequestReceived(SvnMgrSession session, StringRequestInfo requestInfo)
        //{
        //    switch (requestInfo.Key.ToUpper())
        //    {
        //        case ("ADD"):
        //            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SvnArgs>(requestInfo.Body);
        //            if (!data.Bland)
        //            {
        //                data.Pws = Platform.Common.ToolKit.DesEncrypt.DecryptLittle(data.Pws);
        //            }
        //            adding(data);
        //            break;

        //        case ("QUIT"):
        //            if (requestInfo.Body == SSClient.SSKey)
        //            {
        //                session.Send("is quiting");
        //                session.AppServer.Stop();
        //            }
        //            else
        //            {
        //                session.Send(requestInfo.Body);
        //            }
        //            break;
        //    }
        //}

    }

}
