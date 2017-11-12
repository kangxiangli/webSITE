
//  <copyright file="RegistryBaseKey.cs" company="优维拉软件设计工作室">



//  <last-date>2014-07-18 18:52</last-date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Whiskey.Utility.Windows
{
    /// <summary>
    /// 注册表基项域
    /// </summary>
    public enum RegistryBaseKey
    {
        /// <summary>
        /// 对应于HKEY_CLASSES_ROOT主键
        /// </summary>
        ClassesRoot,

        /// <summary>
        /// 对应于HKEY_CURRENT_USER主键
        /// </summary>
        CurrentUser,

        /// <summary>
        /// 对应于 HKEY_LOCAL_MACHINE主键
        /// </summary>
        LocalMachine,

        /// <summary>
        /// 对应于 HKEY_USER主键
        /// </summary>
        Users,

        /// <summary>
        /// 对应于HEKY_CURRENT_CONFIG主键
        /// </summary>
        CurrentConfig
    }
}