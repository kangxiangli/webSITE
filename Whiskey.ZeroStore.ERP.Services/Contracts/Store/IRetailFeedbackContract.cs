
using System.Collections.Generic;
using System.Linq;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IRetailProductFeedbackContract : IDependency
    {
        IQueryable<RetailProductFeedback> Entities { get; }

        RetailProductFeedback View(int Id);

        OperationResult Insert(params RetailProductFeedback[] entities);

        OperationResult Update(params RetailProductFeedback[] entities);
        /// <summary>
        /// 启用或禁用数据
        /// </summary>
        /// <param name="enable">true启动,false禁用</param>
        /// <param name="ids">数据Ids</param>
        /// <returns></returns>
        OperationResult EnableOrDisable(bool enable, params int[] ids);
        /// <summary>
        /// 删除或还原数据
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecovery(bool delete, params int[] ids);


        OperationResult Delete(params RetailProductFeedback[] entities);

        OperationResult Update(ICollection<RetailProductFeedback> entities);

        RetailProductFeedback Edit(int id);
        List<RetailProductFeedbackOptionDto> GetOptions();
        OperationResult UpdateOptions(List<RetailProductFeedbackOptionDto> options);
        /// <summary>
        /// 提交试穿反馈信息
        /// </summary>
        /// <param name="appointmentId">预约id</param>
        /// <param name="productNumber">商品货号</param>
        /// <param name="feedbacks">反馈信息</param>
        /// <returns></returns>
        OperationResult SubmitFeedbacks(int memberId, string retailNumber, List<RetailProductFeedbackEntry> entries);

    }
}






