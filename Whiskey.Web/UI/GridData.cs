
//  <copyright file="GridData.cs" company="优维拉软件设计工作室">



//  <last-date>2014-07-22 21:45</last-date>


using System.Collections.Generic;


namespace Whiskey.Web.UI
{
    /// <summary>
    /// 列表数据，封装列表的行数据与总记录数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridData<T>
    {
        public GridData()
            : this(new List<T>(), 0)
        { }

        public GridData(IEnumerable<T> rows, int total)
        {
            Rows = rows;
            Total = total;
        }

        /// <summary>
        /// 获取或设置 行数据
        /// </summary>
        public IEnumerable<T> Rows { get; set; }

        /// <summary>
        /// 获取或设置 数据行数
        /// </summary>
        public int Total { get; set; }
    }
}