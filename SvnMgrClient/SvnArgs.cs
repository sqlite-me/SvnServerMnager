using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnMgrClient
{
    public class CreateUserArgs
    {
        /// <summary>
        /// Add user for the Repository, if not exist ,create it auto
        /// </summary>
        public string RepositoryName { get; set; }
        /// <summary>
        /// User account
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// pass word
        /// </summary>
        public string PassWord { get; set; }
        /// <summary>
        /// if the pass word is encoded , config the decode method in "PWD_DECODE_CLASS" and "PWD_DECODE_METHOD" and implement this method
        /// </summary>
        public bool PassWordCoded { get; set; }
    }
}
