using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace VietphraseMixHTML
{
    public static class GlobalHttpClient
    {
        private static HttpClient httpClient = null;
        public static HttpClient Instance
        {
            get
            {
                if (httpClient == null) { httpClient= new HttpClient(new HttpClientHandler { UseDefaultCredentials = true }); }
                return httpClient;
            }
        }
    }
}
