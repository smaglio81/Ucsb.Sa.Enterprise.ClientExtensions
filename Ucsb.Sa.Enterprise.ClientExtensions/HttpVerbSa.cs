using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
    /// <summary>
    /// Just used to avoid magic strings in the code. Pretty unnecessary.
    /// </summary>
    public struct HttpVerbSa
    {
        public const string Get = "GET";
        public const string Put = "PUT";
        public const string Post = "POST";
        public const string Delete = "DELETE";
    }
}
