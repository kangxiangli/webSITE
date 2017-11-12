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
    /// Grid查询请求
    /// </summary>
    public class GridRequest
    {
        /// <summary>
        /// 初始化一个<see cref="GridRequest"/>类型的新实例
        /// </summary>
        public GridRequest(HttpRequestBase request)
        {
            string jsonWhere = request.Params["Conditions"];
            FilterGroup = !jsonWhere.IsNullOrEmpty() ? JsonHelper.FromJson<FilterGroup>(jsonWhere) : new FilterGroup();
            RequestInfo = request;
            int pageIndex = request.Params["iDisplayStart"].CastTo(1);
            int pageSize = request.Params["iDisplayLength"].CastTo(5);
            PageCondition = new PageCondition(pageIndex, pageSize);
            string columns = request.Params["sColumns"].CastTo("");
            int columnIndex = request.Params["iSortCol_0"].CastTo(0);
            if (columns!=null && columns.Length > 0)
            {
                var columnArray = columns.Split(',');
                var sortField = columnArray[columnIndex];
                if (sortField != null)
                {
                    string sortOrder = request.Params["sSortDir_0"];
                    if (!sortField.IsNullOrEmpty() && !sortOrder.IsNullOrEmpty())
                    {
                        string[] fields = sortField.Split(",", true);
                        string[] orders = sortOrder.Split(",", true);
                        if (fields.Length != orders.Length)
                        {
                            throw new ArgumentException("查询列表的排序参数个数不一致。");
                        }
                        List<SortCondition> sortConditions = new List<SortCondition>();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            ListSortDirection direction = orders[i].ToLower() == "desc"
                                ? ListSortDirection.Descending
                                : ListSortDirection.Ascending;
                            sortConditions.Add(new SortCondition(fields[i], direction));
                        }
                        PageCondition.SortConditions = sortConditions.ToArray();
                    }
                    else
                    {
                        PageCondition.SortConditions = new SortCondition[] { };
                    }
                }
            }




        }

        /// <summary>
        /// 获取 查询条件组
        /// </summary>
        public FilterGroup FilterGroup { get;  set; }

        /// <summary>
        /// 获取 分页查询条件信息
        /// </summary>
        public PageCondition PageCondition { get;  set; }


        /// <summary>
        /// 获取 请求附加信息
        /// </summary>
        public HttpRequestBase RequestInfo { get;  set; }
    }

}