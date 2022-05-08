using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MCModpackInstaller
{
    // THIS WHOLE CLASS IS FOR DEBUG ONLY, DID NOT USE IN ACTUAL DEPLOYMENT
    // 0 REFERENCES TO OTHER CLASSES
    class MyWebClient : WebClient
    {
        CookieContainer c = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri u)
        {
            var r = (HttpWebRequest)base.GetWebRequest(u);
            r.CookieContainer = c;
            return r;
        }
    }
}
