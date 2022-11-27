using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrApp
{
     class PwdDecode
    {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        public static string DecodePwd(string pwd)
        {
            try
            {
                if (_method == null)
                    _method = getMethod();

                var rlt = _method.Invoke(null, new object[] { pwd })?.ToString();
                if (rlt != null)
                    return rlt;
            }
            catch (Exception exc)
            {
                logger.Error(exc);
            }
            return pwd;
        }

        private static MethodInfo getMethod()
        {
           var className = ConfigurationManager.AppSettings["PWD_DECODE_CLASS"];
           var methodName = ConfigurationManager.AppSettings["PWD_DECODE_METHOD"];
            var index = className.LastIndexOf('.');
            var assName = className.Substring(0, index);
            var assembly = Assembly.Load(assName);
            var type = assembly.GetType(className);
            var method = type.GetMethod(methodName);
            return method;
        }
        private static MethodInfo _method;
    }
}
