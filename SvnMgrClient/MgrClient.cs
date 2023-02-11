using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrClient
{
    public class MgrClient:IDisposable
    {
        public const string SSKey = "dfsf15ewrw212sdfhgj1ew4";
        private const string MgrPortSplite = "MgrPort:";
        private readonly string _hostIp;
        private readonly int _port;
        private EasyClient<JsonCmdPackageInfo> client;
        public event Action<object, JsonCmdPackageInfo> PackageRecived;
        private bool disposed;

        public string RepositoryDefault { get; private set; }

        public MgrClient(string hostIp,int port)
        {
            _hostIp = hostIp;
            _port = port;
            init();
        }

        public MgrClient(string svnRoot)
        {
            _hostIp = GetSvnHost(svnRoot);
            var port = GetMgrPort(svnRoot);
            _port = port??0;
            init();

            RepositoryDefault = GetSvnRepository(svnRoot);
        }

        private void init()
        {
            if (string.IsNullOrEmpty(_hostIp))
            {
                throw new NotSupportedException("no Host IP");
            }
            if (_port==0)
            {
                throw new NotSupportedException("not contain " + MgrPortSplite);
            }
            client = new SuperSocket.ClientEngine.EasyClient<JsonCmdPackageInfo>();
            client.Initialize(new ReceiveFilter());
            client.NewPackageReceived += Client_NewPackageReceived;
        }

        private void Client_NewPackageReceived(object sender, PackageEventArgs<JsonCmdPackageInfo> e)
        {
            PackageRecived?.Invoke(this,e?.Package);
        }

        public static string GetSvnUri(string svnRoot)
        {
            return svnRoot?.Split(';')?.FirstOrDefault();
        }

        private static int? GetMgrPort(string svnRoot)
        {
            var tmp = svnRoot?.Split(';')?.FirstOrDefault(t => t.StartsWith(MgrPortSplite, StringComparison.OrdinalIgnoreCase));
            if (tmp == null) return null;

            tmp = tmp?.Substring(MgrPortSplite.Length)?.Trim(' ', ';', ':');

            return int.TryParse(tmp, out int port) ? (int?)port : null;
        }

        public static string GetSvnHost(string svnRoot)
        {
            return svnRoot?.Split(new[] { "://" }, 2, StringSplitOptions.None)
                ?.LastOrDefault()?.Split(new[] { ':', '/' },2)?.FirstOrDefault();
        }
        
        public static string GetSvnRepository(string svnRoot)
        {
            var arry = svnRoot?.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)?.ToArray();
            if (arry?.Length > 0)
            {
                var svn = Array.FindIndex(arry, t => t.Equals("svn", StringComparison.OrdinalIgnoreCase));
                if (svn >0&&svn+1<arry.Length)
                {
                    return arry[svn + 1];
                }
            }

            return null;
        }

        ~MgrClient() {
            Dispose(false);
        }

        public bool Connect()
        {
            var task = client.ConnectAsync(new IPEndPoint(IPAddress.Parse(_hostIp), _port));
            task.Wait();
            if (task.Result)
            {
                return "success" == Send("LOGIN",SSKey, 0);
            }
            return false;
        }

        public string Send(string cmd,object data,int timeOutSecond = 30)
        {
            var package = new JsonCmdPackageInfo(cmd, data, Guid.NewGuid().ToString());
            package.Send(client);
            JsonCmdPackageInfo received = null;
            PackageRecived += recive;

            var time = DateTime.Now;
            while (received == null)
            {
                Task.Delay(100).Wait();
                if (timeOutSecond>0&& (DateTime.Now - time).TotalSeconds > timeOutSecond) break;
            }
            PackageRecived -= recive;
            if (received == null)
                throw new TimeoutException("Send commond time out");

            return received?.Body;

            void recive(object s, JsonCmdPackageInfo rPackage)
            {
                if (rPackage.Cmd == package.Cmd && rPackage.Id == package.Id)
                    received = rPackage;
            }
        }

        public bool AddUser(CreateUserArgs svnArgs, int timeOutSecond = 30)
        {
            if (svnArgs == null)
                throw new ArgumentNullException(nameof(svnArgs));

            if (string.IsNullOrEmpty(svnArgs.Account))
            {
                throw new ArgumentNullException(nameof(svnArgs) + "." + nameof(svnArgs.Account));
            }
            if (string.IsNullOrEmpty(svnArgs.PassWord))
            {
                throw new ArgumentNullException(nameof(svnArgs) + "." + nameof(svnArgs.PassWord));
            }

            if (string.IsNullOrEmpty(svnArgs.RepositoryName))
            {
                svnArgs.RepositoryName = RepositoryDefault;
            }

            if (string.IsNullOrEmpty(svnArgs.RepositoryName))
            {
                throw new ArgumentNullException(nameof(svnArgs) + "." + nameof(svnArgs.RepositoryName));
            }

            var rlt = Send("ADD", svnArgs, timeOutSecond);
            var sucessed=rlt== "success";
            if (!sucessed)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"svn AddUser {svnArgs?.Account}({svnArgs.RepositoryName}) error:{rlt}");
            }
            return sucessed;
        }

        public bool Close()
        {
            var task = client.Close();
            return task.Result;
        }

        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收器不再调用终结器
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposed) return;
            //清理非托管资源
            try { Close(); } catch { }
            //if (NativeResource != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(NativeResource);
            //    NativeResource = IntPtr.Zero;
            //}

            //清理托管资源
            if (disposing)
            {
                //if (ManagedResource != null)
                //{
                //    ManagedResource = null;
                //}
            }
            disposed = true;
        }
    }
}
