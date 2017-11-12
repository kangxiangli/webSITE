using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using Whiskey.Utility.Data;
using Whiskey.Utility.Exceptions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Web
{
    /// <summary>
    /// 列表数据，封装列表的行数据与总记录数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridData<T>
    {
        public GridData(IEnumerable<T> list, int count, HttpRequestBase request)
        {
            sEcho = request.Params["sEcho"].CastTo("0");
            iDisplayStart = request.Params["iDisplayStart"].CastTo(0);
            iTotalRecords = count;
            iTotalDisplayRecords = count;
            aaData = list;
        }

        /// <summary>
        /// DataTable请求服务器端次数
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// 分页时每页跨度数量
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// 每页显示的数量
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// 分页总数量
        /// </summary>
        public int iTotalRecords { get; set; }

        /// <summary>
        /// 分页时每页总显示数量
        /// </summary>
        public int iTotalDisplayRecords { get; set; }

        /// <summary>
        /// 获取或设置 行数据
        /// </summary>
        public IEnumerable<T> aaData { get; set; }

    }
    //yxk 2015-12
    public class GridDataResul<T> : GridData<T>
    {
        public GridDataResul(IEnumerable<T> list, int count, HttpRequestBase request)
            : base(list, count, request) { }
        public object Other { get; set; }
    }

}