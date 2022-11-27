using Microsoft.VisualStudio.TestTools.UnitTesting;
using SvnMgrClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrClient.Tests
{
    [TestClass()]
    public class MgrClientTests
    {
        [TestMethod()]
        public void AddUserTest()
        {
            using (var client = new MgrClient("https://39.96.77.143:9209/svn/local20_10/;MgrPort:9205"))
            {
                client.Connect();
              var rlt =   client.AddUser(new CreateUserArgs
                {
                    Account = "221127",
                    PassWord = "AQEAFCER",
                    PassWordCoded = true
                });
            Assert.IsTrue(rlt);
            }
        }
    }
}