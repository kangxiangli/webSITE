using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;

namespace Whiskey.ZeroStore.ERP.Winform.Extensions
{
    public static class EnvironmentHelper
    {

        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <param name="IsLocal">是否获取本地IP</param>
        /// <returns></returns>
        public static string GetIP()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }


        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMAC()
        {
            string MacAddress = String.Empty;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    MacAddress = mo["MacAddress"].ToString();
                    break;
                }
            }
            moc = null;
            mc = null;
            MacAddress = MacAddress.Replace(":", "-");
            return MacAddress;
        }

    }
}
