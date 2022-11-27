using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{/// <summary>
 ///     Visual SVN 操作辅助类：使用WMI控制VisualSVN，MOF文件存在位置：C:\Program Files\VisualSVN Server\WMI\VisualSVNServer.mof
 /// </summary>
    public class SVNHelper
    {
        #region 设置

        #region 设置仓库权限

        public enum AccessLevel : uint
        {
            NoAccess = 0,
            ReadOnly,
            ReadWrite
        }

        /// <summary>
        ///     设置仓库权限（给用户授权）
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="repository">SVN仓库</param>
        public static bool SetRepositoryPermission(string userName, string repository)
        {
            return SetRepositoryPermission(userName, repository, 2);
        }

        /// <summary>
        ///     设置仓库权限（给用户授权）
        /// </summary>
        /// <param name="users">用户名</param>
        /// <param name="repository">SVN仓库</param>
        public static bool SetRepositoryPermission(List<string> users, string repository)
        {
            string userNames = "";
            foreach (string user in users)
            {
                userNames += user + ",";
            }
            return SetRepositoryPermission(userNames, repository, 2);
        }

        /// <summary>
        ///     设置仓库权限（给用户授权）
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="repository">SVN仓库</param>
        /// <param name="permission"> 权限码：0拒绝，1只读，2读写</param>
        public static bool SetRepositoryPermission(string userName, string repository, int permission)
        {
            try
            {
                string[] users = userName.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                IDictionary<string, AccessLevel> permissions = GetPermissions(repository, "/");
                foreach (string s in users)
                {
                    if (!permissions.ContainsKey(s))
                    {
                        permissions.Add(s, AccessLevel.ReadWrite);
                    }
                }
                SetPermissions(repository, "/", permissions);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     根据仓库名取得仓库实体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ManagementObject GetRepositoryObject(string name)
        {
            return new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_Repository.Name='{0}'", name), null);
        }

        /// <summary>
        ///     读取权限实体
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="accessLevel"></param>
        /// <returns></returns>
        private static ManagementObject GetPermissionObject(string sid, AccessLevel accessLevel)
        {
            var accountClass = new ManagementClass("root\\VisualSVN",
                                                   "VisualSVN_WindowsAccount", null);
            var entryClass = new ManagementClass("root\\VisualSVN",
                                                 "VisualSVN_PermissionEntry", null);
            ManagementObject account = accountClass.CreateInstance();
            if (account != null) account["SID"] = sid;
            ManagementObject entry = entryClass.CreateInstance();
            if (entry != null)
            {
                entry["AccessLevel"] = accessLevel;
                entry["Account"] = account;
                return entry;
            }
            return null;
        }

        /// <summary>
        ///     设置仓库权限
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <param name="path"></param>
        /// <param name="permissions"></param>
        private static void SetPermissions(string repositoryName, string path,
                                           IEnumerable<KeyValuePair<string, AccessLevel>> permissions)
        {
            ManagementObject repository = GetRepositoryObject(repositoryName);
            ManagementBaseObject inParameters = repository.GetMethodParameters("SetSecurity");
            inParameters["Path"] = path;
            IEnumerable<ManagementObject> permissionObjects =
                permissions.Select(p => GetPermissionObject(p.Key, p.Value));
            inParameters["Permissions"] = permissionObjects.ToArray();
            repository.InvokeMethod("SetSecurity", inParameters, null);
        }

        public static IDictionary<string, AccessLevel> GetPermissions(string repositoryName)
        {
            var tmp = GetPermissions(repositoryName, "/");
            return tmp;
        }

        /// <summary>
        ///     读取仓库权限
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static IDictionary<string, AccessLevel> GetPermissions(string repositoryName, string path)
        {
            ManagementObject repository = GetRepositoryObject(repositoryName);
            ManagementBaseObject inParameters = repository.GetMethodParameters("GetSecurity");
            inParameters["Path"] = path;
            ManagementBaseObject outParameters = repository.InvokeMethod("GetSecurity", inParameters, null);

            var permissions = new Dictionary<string, AccessLevel>();

            if (outParameters != null)
                foreach (ManagementBaseObject p in (ManagementBaseObject[])outParameters["Permissions"])
                {
                    // NOTE: This will fail if VisualSVN Server is configured to use Subversion
                    // authentication.  In that case you'd probably want to check if the account
                    // is a VisualSVN_WindowsAccount or a VisualSVN_SubversionAccount instance
                    // and tweak the property name accordingly. 
                    var account = (ManagementBaseObject)p["Account"];
                    var sid = (string)account["SID"];
                    var accessLevel = (AccessLevel)p["AccessLevel"];

                    permissions[sid] = accessLevel;
                }

            return permissions;
        }

        #endregion

        #region 创建用户组

        /// <summary>
        ///     创建用户组
        /// </summary>
        public static bool CreatGroup(string groupName)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create");
                @params["Name"] = groupName.Trim();
                @params["Members"] = new object[] { };
                svn.InvokeMethod("Create", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 创建用户

        /// <summary>
        ///     创建用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CreateUser(string userName, string password)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_User", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create");
                @params["Name"] = userName.Trim();
                @params["Password"] = password.Trim();
                svn.InvokeMethod("Create", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region  创建svn仓库

        /// <summary>
        ///     创建svn仓库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CreateRepository(string name)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create"); //创建方法参数引用
                @params["Name"] = name.Trim(); //传入参数
                svn.InvokeMethod("Create", @params, null); //执行
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region  创建svn仓库目录

        /// <summary>
        ///     创建svn仓库目录
        /// </summary>
        /// <param name="repositories"> </param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CreateRepositoryFolders(string repositories, string[] name)
        {
            try
            {
                var repository = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementObject repoObject = repository.CreateInstance();
                if (repoObject != null)
                {
                    repoObject.SetPropertyValue("Name", repositories);
                    ManagementBaseObject inParams = repository.GetMethodParameters("CreateFolders");
                    inParams["Folders"] = name;
                    inParams["Message"] = "";
                    repoObject.InvokeMethod("CreateFolders", inParams, null);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        //public static bool SetMemberGroup(string userName, string groupName)
        //{
        //    try
        //    {
        //        var userObj = GetGroupUsersArr(groupName);
        //        foreach (ManagementBaseObject baseObject in userObj)
        //        {
        //            if (baseObject["Name"].ToString().ToLower() == userName)
        //            {
        //                return false;
        //            }
        //        }
        //        var addUser = new ManagementClass("root\\VisualSVN", "VisualSVN_SubversionAccount", null).CreateInstance();
        //        if (addUser != null)
        //        {
        //            addUser.SetPropertyValue("Name", userName);
        //            userObj.Add(addUser);
        //        }

        //        var svnUser = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
        //        ManagementBaseObject inParams = svnUser.GetMethodParameters("SetMembers");
        //        inParams["Members"] = new object[] { userObj };
        //        svnUser.InvokeMethod("SetMembers", inParams, null);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        #endregion

        #region 读取

        /// <summary>
        ///     读取指定组里的用户
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static List<string> GetGroupUsers(string groupName)
        {
            var listUsers = new List<string>();
            var group = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
            ManagementObject instance = group.CreateInstance();
            if (instance != null)
            {
                instance.SetPropertyValue("Name", groupName.Trim());
                ManagementBaseObject outParams = instance.InvokeMethod("GetMembers", null, null); //通过实例来调用方法
                if (outParams != null)
                {
                    var members = outParams["Members"] as ManagementBaseObject[];
                    if (members != null)
                    {
                        foreach (ManagementBaseObject member in members)
                        {
                            object name = member["Name"];
                            listUsers.Add(name.ToString());
                        }
                    }
                }
            }
            return listUsers;
        }

        public static List<ManagementBaseObject> GetGroupUsersArr(string groupName)
        {
            var list = new List<ManagementBaseObject>();
            var group = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
            ManagementObject instance = group.CreateInstance();
            if (instance != null)
            {
                instance.SetPropertyValue("Name", groupName.Trim());
                ManagementBaseObject outParams = instance.InvokeMethod("GetMembers", null, null); //通过实例来调用方法
                if (outParams != null)
                {
                    var members = outParams["Members"] as ManagementBaseObject[];
                    if (members != null)
                    {
                        foreach (ManagementBaseObject member in members)
                        {
                            list.Add(member);
                        }
                    }
                }
            }
            return list;
        }

        #endregion

    }
}
