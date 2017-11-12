
//  <copyright file="OperationResultType.cs" company="优维拉软件设计工作室">


//  <last-editor>最后修改人</last-editor>
//  <last-date>2014-07-30 4:36</last-date>


using System.ComponentModel;


namespace Whiskey.Utility.Data
{
    /// <summary>
    /// 表示业务操作结果的枚举
    /// </summary>
    public enum OperationResultType
    {
        /// <summary>
        ///   输入信息验证失败
        /// </summary>
        [Description("输入信息验证失败。")]
        ValidError = 0,

        /// <summary>
        ///   指定参数的数据不存在
        /// </summary>
        [Description("指定参数的数据不存在。")]
        QueryNull = 1,

        /// <summary>
        ///   操作取消或操作没引发任何变化
        /// </summary>
        [Description("操作没有引发任何变化，提交取消。")]
        NoChanged = 2,

        /// <summary>
        ///   操作成功
        /// </summary>
        [Description("操作成功。")]
        Success = 3,

        /// <summary>
        ///   操作引发错误
        /// </summary>
        [Description("操作引发错误。")]
        Error = 4,

        /// <summary>
        /// 名称重复
        /// </summary>
        [Description("名称已经存在")]
        NameRepeat = 5,

        /// <summary>
        /// 登录异常
        /// </summary>
        [Description("登录异常")]
        LoginError = 6,

        /// <summary>
        /// 数据重复
        /// </summary>
        [Description("数据已经存在")]
        DataRepeat = 7
    }
}