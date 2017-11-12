
//  <copyright file="NextSearchMode.cs" company="优维拉软件设计工作室">
//      Copyright (c) 2015 www.ovisa.cn All rights reserved.


//  <last-date>2015-01-02 12:29</last-date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Whiskey.Web.Net.WebPull.Images
{
    /// <summary>
    /// 下一个数据搜索模式
    /// </summary>
    public enum NextSearchMode
    {
        /// <summary>
        /// 循环模式，知道总数，使用循环遍历
        /// </summary>
        Cycle,

        /// <summary>
        /// 逐个查询，从当前数据中获取下一数据的标识，一个一个获取
        /// </summary>
        OneByOne
    }
}