using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmileShop.Authorization.Configurations
{
    public class ServiceURL
    {
        public string ShortLinkApi { get; set; }
        public string SendSmsApi { get; set; }
        public bool SendSmsApiEnable { get; set; } = false;
    }
}