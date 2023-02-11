using NLog;
using SuperSocket.SocketBase.Command;
using SvnMgrClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp.Command
{
   public class Cmd_AddUser : CommandBase<SvnMgrSession, JsonCmdPackageInfo>
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public override string Name => "ADD";
        public override void ExecuteCommand(SvnMgrSession session, JsonCmdPackageInfo requestInfo)
        {
            if (session.Logined)
            {
                logger?.Info("Execute ADD ", requestInfo?.Body);
                if (adding(requestInfo.Body))
                {
                    session.Success(requestInfo);
                    logger?.Info("Execute ADD Success", requestInfo);
                }
                else
                {
                    session.Error(requestInfo);
                    logger?.Error("Execute ADD Error", requestInfo);
                }
            }
            else
            {
                requestInfo.Send(session);
                logger?.Error("Execute ADD not login", requestInfo?.Body);
            }
        }


        private bool adding(string json)
        {
            try
            {
                CreateUserArgs data = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateUserArgs>(json);
                var repName = data.RepositoryName;
                var account = data.Account;
                var psd = data.PassWord;
                if (data.PassWordCoded)
                {
                    psd =  PwdDecode.DecodePwd(psd);
                }
                var existRepName = SvnHelper2.GetRepository(repName);
                if (existRepName != repName)
                {
                    logger.Debug("正在创建仓库：" + repName);
                    existRepName = SvnHelper2.NewRepository(repName);
                    if (existRepName == repName)
                    {
                        logger.Debug("成功创建仓库：" + repName);
                    }
                    else
                    {
                        logger.Debug("未能创建仓库：" + repName);
                    }
                }

                var existUser = SvnHelper2.GetUser(account);
                if (existRepName != account)
                {
                    logger.Debug("正在创建账号：" + account);
                    var baseDir = SvnHelper2.GetBaseDir();
                    SvnHelper2.AddUser(account, psd, baseDir);
                    existUser = SvnHelper2.GetUser(account);
                    if (existUser == account)
                    {
                        logger.Debug("成功创建账号：" + account);
                    }
                    else
                    {
                        logger.Debug("未能创建账号：" + account);
                    }
                }


                var success = SvnHelper2.AddAccessRule(repName, account);
                if (success)
                {
                    logger.Debug($"{account} 成功分配权限到 {repName}");
                }
                else
                {
                    logger.Debug($"{account} 未能分配权限到 {repName}");
                }

                return existUser == account;
            }
            catch (Exception exc)
            {
                logger.Error(exc);
            }
            return false;
        }

    }
}
