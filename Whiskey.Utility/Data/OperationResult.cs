
using System;

namespace Whiskey.Utility.Data
{
    /// <summary>
    /// 业务操作结果信息类，对操作结果进行封装
    /// </summary>
    public class OperationResult : OperationResult<object>
    {
        #region 构造函数

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult()
            : this(OperationResultType.Error, null, null)
        { }
        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType)
            : this(resultType, null, null)
        { }

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message)
            : this(resultType, message, null)
        { }

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message, object data)
            : base(resultType, message, data)
        { }


        /// <summary>
        /// 操作成功
        /// </summary>
        /// <param name="successMsg"></param>
        /// <returns></returns>
        public static OperationResult OK(string successMsg)
        {
            return new OperationResult(OperationResultType.Success, successMsg ?? "ok", null);
        }

        /// <summary>
        /// 操作成功
        /// </summary>
        /// <returns></returns>
        public static OperationResult OK()
        {
            return new OperationResult(OperationResultType.Success, "ok", null);
        }
        /// <summary>
        /// 操作成功
        /// </summary>
        /// <returns></returns>
        public static OperationResult OK(object data)
        {
            return new OperationResult(OperationResultType.Success, "ok", data);
        }


        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="successMsg"></param>
        /// <returns></returns>
        public static OperationResult Error(string errMsg)
        {
            return new OperationResult(OperationResultType.Error, errMsg ?? "操作失败", null);
        }

        #endregion
    }


    public class PagedOperationResult : OperationResult<object>
    {
        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public PagedOperationResult(OperationResultType resultType, string message, object data)
            : base(resultType, message, data)
        { }


        public int PageSize { get; set; }
        public int PageCount { get {
                return (int)Math.Ceiling(AllCount * 1.0 / PageSize);
            } }
        public int AllCount { get; set; }
    }


    /// <summary>
    /// 泛型版本的业务操作结果信息类，对操作结果进行封装
    /// </summary>
    /// <typeparam name="T">返回数据的类型</typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// 初始化一个<see cref="OperationResult{T}"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType)
            : this(resultType, null, default(T))
        { }

        /// <summary>
        /// 初始化一个<see cref="OperationResult{T}"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message)
            : this(resultType, message, default(T))
        { }

        /// <summary>
        /// 初始化一个<see cref="OperationResult{T}"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message, T data)
        {
            ResultType = resultType;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// 获取或设置 操作结果类型
        /// </summary>
        public OperationResultType ResultType { get; set; }

        /// <summary>
        /// 获取或设置 操作返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置 操作返回数据
        /// </summary>
        public T Data { get; set; }

        public T Other { get; set; }
    }

}