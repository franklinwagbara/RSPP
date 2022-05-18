using UAParser;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RSPP.Helpers
{

    public struct ParameterData
    {

        public ParameterData(string Key, string Value)
        {
            ParamKey = Key;
            ParamValue = Value;
        }

        public string ParamKey { get; set; }
        public string ParamValue { get; set; }

    }


    //public class AppModels
    //{
    //    public CompanyDetail companyDetail { get; set; }
    //}




    public class Authentications
    {
        
        public string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    properties.GetIPv4Properties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }
        

        public string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }



        public string BrowserName(HttpContext httpContext)
        {
            //HttpContext context = null;
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);

            return c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;
        }


        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var Ip = "";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {   
                    Ip = ip.ToString();
                }
                else
                {
                    Ip = ip.ToString();
                }
            }
            return Ip;
        }

    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get remote ip address, optionally allowing for x-forwarded-for header check
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="allowForwarded">Whether to allow x-forwarded-for header check</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }
            return context.Connection.RemoteIpAddress;
        }
    }


}
