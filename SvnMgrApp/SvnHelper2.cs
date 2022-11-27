using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{
    class SvnHelper2
    {
        private static PowerShell ps = PowerShell.Create();
        public static StringBuilder ExecutScript(params string[] scripts)
        {
            ps.Commands.Clear();
            foreach (var one in scripts)
                ps.AddScript(one);

            ps.AddCommand("Out-String");
            var rlt = ps.Invoke();

            var sb = new StringBuilder();
            foreach(var one in rlt)
            {
                sb.AppendLine(one.ToString());
            }
            return sb;
        }

        public static string GetBaseDir() {
            ps.Commands.Clear();
            ps.AddScript("Get-SvnServerConfiguration");
            var psObjArr = ps.Invoke();
            return ((dynamic)psObjArr[0]).RepositoriesRoot;
        }

        public static string GetRepository(string repName) {
            ps.Commands.Clear();
            ps.AddScript("Get-SvnRepository "+repName);
            var psObjArr = ps.Invoke();
            try
            {
                return ((dynamic)psObjArr[0]).Name;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string NewRepository(string repName)
        {
            ps.Commands.Clear();
            ps.AddScript("New-SvnRepository " + repName);
            var psObjArr = ps.Invoke();

            return ((dynamic)psObjArr[0]).Name;
        }


        public static string GetUser(string user)
        {
            ps.Commands.Clear();
            ps.AddScript("Get-SvnLocalUser " + user);
            var psObjArr = ps.Invoke();

            try
            {
                return ((dynamic)psObjArr[0]).Id;
            }catch(Exception exc)
            {
                return null;
            }
        }

        public static void AddUser(string user, string psd, string repBaseDir)
        {
            ps.Commands.Clear();
            ps.AddScript($@".\htpasswd.exe -b {repBaseDir}htpasswd {user} {psd}");
            var psObjArr = ps.Invoke();
        }

        public static string DelUser(string user, string psd, string repBaseDir)
        {
            var rlt = ExecutScript(new[] {
            $@".\htpasswd.exe -rb {repBaseDir}htpasswd {user} {psd}"
            });

            return rlt?.ToString();
        }

        public static bool AddAccessRule(string rp, string user, AccessType accessType= AccessType.ReadWrite)
        {
            ps.Commands.Clear();
            ps.AddScript($@"Add-SvnAccessRule {rp} -Path / -AccountName {user} -Access {accessType}");
            var psObjArr = ps.Invoke();

            return psObjArr.Count > 0;
        }
    }

    public enum AccessType
    {
        ReadWrite,
        ReadOnly
    }
}
