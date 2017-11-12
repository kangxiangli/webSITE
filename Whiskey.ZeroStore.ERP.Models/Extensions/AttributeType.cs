using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Extensions
{
    /// <summary>
    /// 属性类型
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// 文字标签
        /// </summary>
        Label = 1,
        /// <summary>
        /// 下拉列表
        /// </summary>
        DropdownList = 2,
        /// <summary>
        /// 单选列表
        /// </summary>
        RadioList = 3,
        /// <summary>
        /// 多选列表
        /// </summary> 
        Checkbox = 4,
        /// <summary>
        /// 文本输入框
        /// </summary>
        TextBox = 5,
        /// <summary>
        /// 多行文本输入框
        /// </summary>
        MultilineTextbox = 10,
        /// <summary>
        /// 时间选择器
        /// </summary>
        TimePicker = 20,
        /// <summary>
        /// 日期选择器
        /// </summary>
        DatePicker = 30,
        /// <summary>
        /// 范围选择器
        /// </summary>
        RangePicker = 40,
        /// <summary>
        /// 文件上传器
        /// </summary>
        FileUploader = 50,
        /// <summary>
        /// 颜色选择器
        /// </summary>
        ColorSquares = 60,
        /// <summary>
        /// 无限级选择器
        /// </summary>
        LimitlessSelector = 70,


    }

}
