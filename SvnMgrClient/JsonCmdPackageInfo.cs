using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrClient
{
    public class JsonCmdPackageInfo : SuperSocket.ProtoBase.IPackageInfo
    {
        public JsonCmdPackageInfo(byte[] header, byte[] bodyBuffer)
        {
            Header = header;
            Data = bodyBuffer;
            var str = UTF8Encoding.UTF8.GetString(Data);
            if (IsCmd)
            {
                var arr = str.Split(new[] { ' ' },2);
                if (arr.Length > 0)
                {
                    if (arr.Length > 1) Body = arr[1];
                    str = arr[0];
                    arr = str.Split(new[] { ':' }, 2); ;
                    if (arr.Length > 0) Cmd = arr[0];
                    if (arr.Length > 1) Id = arr[1];
                }
            }

            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="id">If need response, return the same id value</param>
        public JsonCmdPackageInfo(string cmd, object data = null, string id = null)
        {
            if(!(data is string))
            {
                try
                {
                    data = JsonConvert.SerializeObject(data);
                }
                catch {}
            }

            Header = new byte[4];
            Header[0] = 1;
            Cmd = cmd;
            Id = id;
            var dataSend = cmd;
            if (id != null)
            {
                dataSend += ":" + id;
            }

            if (data != null)
            {
                dataSend += " " + data;
            }

            var arr = UTF8Encoding.UTF8.GetBytes(dataSend);
            Data = arr;
            var s = Math.DivRem(arr.Length, 256, out int r);
            Header[2] = (byte)s;
            Header[3] = (byte)r;
        }

        /// <summary>
        /// 服务器返回的字节数据头部
        /// </summary>
        public byte[] Header { get; private set; }
        /// <summary>
        /// 服务器返回的字节数据
        /// </summary>
        public byte[] Data { get;private set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Cmd { get; private set; }
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// 服务器返回的字符串数据
        /// </summary>
        public string Body { get; private set; }

        public bool IsCmd => Header?.Length > 0 && Header[0] == 1;


        internal void Send(SuperSocket.ClientEngine.EasyClientBase sender)
        {
            sender.Send(new List<ArraySegment<byte>>(2){
                new ArraySegment<byte>(this.Header),
            new ArraySegment<byte>(this.Data) });
        }
    }
}
