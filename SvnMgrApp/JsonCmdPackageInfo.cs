using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{
  public  class JsonCmdPackageInfo : SvnMgrClient.JsonCmdPackageInfo, IRequestInfo
    {
        public JsonCmdPackageInfo(byte[] header, byte[] bodyBuffer) : base(header, bodyBuffer)
        {

        }

        public JsonCmdPackageInfo(string cmd,object data = null,string id=null) : base(cmd, data,id) { }
        public string Key => base.Cmd;

        internal void Send(SvnMgrSession sender)
        {
            sender.Send(new List<ArraySegment<byte>>(2){
                new ArraySegment<byte>(this.Header),
            new ArraySegment<byte>(this.Data) });
        }
    }
}
