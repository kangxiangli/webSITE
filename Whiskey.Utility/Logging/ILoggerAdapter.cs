
//  <copyright file="ILoggerAdapter.cs" company="优维拉软件设计工作室">



//  <last-date>2014-08-10 22:14</last-date>


using System;


namespace Whiskey.Utility.Logging
{
    /// <summary>
    /// 定义日志输出适配器的方法
    /// </summary>
    public interface ILoggerAdapter
    {
        /// <summary>
        /// 由指定类型获取<see cref="ILog"/>日志实例
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        ILog GetLogger(Type type);

        /// <summary>
        /// 由指定名称获取<see cref="ILog"/>日志实例
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns></returns>
        ILog GetLogger(string name);
    }
}