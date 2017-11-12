using System.Threading.Tasks;
using Whiskey.Utility.Data;

namespace System
{
    public struct Operation
    {
        public const string Add = "添加";
        public const string Delete = "删除";
        public const string Recovery = "还原";
        public const string Update = "更新";
        public const string View = "查看";
        public const string Enable = "启用";
        public const string Disable = "禁用";
        public const string Cancel = "撤销";
        public const string RecoveryCancel = "恢复撤销";
    }
    /// <summary>
    /// 操作帮助类
    /// </summary>
    public partial class OperationHelper
    {
        /// <summary>
        /// 返回操作结果
        /// </summary>
        /// <param name="status">成功或失败</param>
        /// <param name="message">操作描述</param>
        /// <returns></returns>
        public static OperationResult ReturnOperationResult(bool status, string opera, object data = null)
        {
            return status ? new OperationResult(OperationResultType.Success, opera + "成功", data) : new OperationResult(OperationResultType.Error, opera + "失败", data);
        }
        /// <summary>
        /// 返回操作结果
        /// </summary>
        /// <param name="status">成功或失败</param>
        /// <param name="message">操作描述</param>
        /// <returns></returns>
        public static OperationResult ReturnOperationResultDIY(OperationResultType status, string Msg = null, object data = null)
        {
            return new OperationResult(status, Msg, data);
        }
        /// <summary>
        /// 返回操作异常结果
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static OperationResult ReturnOperationExceptionResult(Exception ex, string opera, bool isDiy = false)
        {
            if (!isDiy)
            {
                return new OperationResult(OperationResultType.Error, opera + "失败！错误如下：" + ex.Message, ex.ToString());
            }

            return new OperationResult(OperationResultType.Error, opera);
        }
    }

    public partial class OperationHelper
    {
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <param name="failureFunc"></param>
        /// <returns></returns>
        public static TResult Try<TResult>(Func<TResult> func, Func<Exception, TResult> failureFunc)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return failureFunc(ex);
            }
        }
        /// <summary>
        /// 异常处理自动返回错误原因
        /// </summary>
        /// <param name="func">逻辑处理，参数为操作描述</param>
        /// <returns></returns>
        public static OperationResult Try(Func<string, OperationResult> func, string opera)
        {
            return Try(() =>
            {
                return func(opera);
            }, ex =>
            {
                return new OperationResult(OperationResultType.Error, $"{opera}操作异常，错误如下：{ex.Message}{ex.ToString()}");
            });
        }
        /// <summary>
        /// 异常处理自动返回自定义错误原因
        /// </summary>
        /// <param name="func">逻辑处理，参数为操作描述</param>
        /// <returns></returns>
        public static OperationResult Try(Func<OperationResult> func, string exMsg)
        {
            return Try(() =>
            {
                return func();
            }, ex =>
            {
                return new OperationResult(OperationResultType.Error, $"{exMsg}失败");
            });
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="func"></param>
        /// <param name="failureFunc"></param>
        public static void Try(Action func, Action<Exception> failureFunc = null)
        {
            try { func?.Invoke(); }
            catch (Exception ex)
            {
                failureFunc?.Invoke(ex);
            }
        }

        #region 异步处理

        /// <summary>
        /// 异常处理（异步）
        /// </summary>
        /// <param name="func"></param>
        /// <param name="opera"></param>
        /// <returns></returns>
        public static Task<OperationResult> TryAsync(Func<string, OperationResult> func, string opera)
        {
            return Task.Run(() => { return Try(func, opera); });
        }
        /// <summary>
        /// 异常处理（异步）
        /// </summary>
        /// <param name="func"></param>
        /// <param name="failureFunc"></param>
        /// <returns></returns>
        public static Task<TResult> TryAsync<TResult>(Func<TResult> func, Func<Exception, TResult> failureFunc)
        {
            return Task.Run(() => { return Try(func, failureFunc); });
        }
        /// <summary>
        /// 异常处理（异步）
        /// </summary>
        /// <param name="func"></param>
        /// <param name="failureFunc"></param>
        /// <returns></returns>
        public static Task TryAsync(Action func, Action<Exception> failureFunc = null)
        {
            return Task.Run(() => { Try(func, failureFunc); });
        }

        #endregion
    }
}
