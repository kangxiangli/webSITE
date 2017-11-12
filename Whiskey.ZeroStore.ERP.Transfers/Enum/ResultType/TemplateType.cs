using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public enum TemplateType
    {
        /// <summary>
        /// 没有添加上传文件 
        /// </summary>
        Empty = 0,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = 2,
        /// <summary>
        /// 后缀名错误
        /// </summary>
        SuffixError = 3,
        /// <summary>
        /// 保持成功
        /// </summary>
        SaveSuccess = 4,
        /// <summary>
        /// 保存失败
        /// </summary>
        SaveFail = 5,
        /// <summary>
        /// 插入数据库成功
        /// </summary>
        InsertSuccess = 6,
        /// <summary>
        /// 插入数据库失败
        /// </summary>
        InsertFail = 7,
        /// <summary>
        /// 已存在
        /// </summary>
        Repeat = 8,
        /// <summary>
        /// 不存在
        /// </summary>
        NotRepeat = 9,
        /// <summary>
        /// 出现BUG
        /// </summary>
        Error=10,
        /// <summary>
        /// 超长
        /// </summary>
        OverLong=11,
    }
}
