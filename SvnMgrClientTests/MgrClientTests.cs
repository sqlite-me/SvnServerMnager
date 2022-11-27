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
            using (var client = new MgrClient("127.0.0.1",9205))
            {
                client.Connect();
              var rlt =   client.AddUser(new CreateUserArgs
                {
                    Account = "user1",
                    PassWord = "AQEAFCER",
                    PassWordCoded = true
                });
            Assert.IsTrue(rlt);
            }
        }
    }
}