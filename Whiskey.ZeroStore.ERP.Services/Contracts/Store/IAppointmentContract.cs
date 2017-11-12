using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppointmentContract : IDependency
    {
        IQueryable<Appointment> Entities { get; }
        /// <summary>
        /// 新增预约
        /// </summary>
        /// <param name="memberId">会员id</param>
        /// <param name="notes">会员留言</param>
        /// <param name="likeNumbers">中意的货号</param>
        /// <param name="dislikeNumbers">不中意的货号</param>
        /// <param name="checkOptions">预约项</param>
        /// <returns></returns>
        OperationResult Add(int memberId, string notes, string[] likeNumbers, string[] dislikeNumbers, Dictionary<string, string> checkOptions);

        OperationResult Update(params Appointment[] entities);
        OperationResult UpdateState(Dictionary<int, string> ids);
        Dictionary<string, object> GetItems(int memberId, int pageIndex = 1, int pageSize = 10, AppointmentState? stat = null);
        OperationResult BatchInsert(IEnumerable<Appointment> entities);

        OperationResult GetOptions();
        OperationResult GetPlans(int id);
        OperationResult GetPlans(string number);
        OperationResult ConfirmPlans(int id, int planId, DateTime start, DateTime end);
        OperationResult RejectAllPlans(int id);

        /// <summary>
        /// 取消预约
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        OperationResult Cancel(int id);
        Task<OperationResult> GetLikes(string number);
        int GetPackingId(int id);
        Task<OperationResult> GetBoxToAccept(int storeId, AppointmentState filter);
        /// <summary>
        /// 快速预约
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        OperationResult QuickAdd(int memberId, DateTime start, DateTime end);

    }
}
