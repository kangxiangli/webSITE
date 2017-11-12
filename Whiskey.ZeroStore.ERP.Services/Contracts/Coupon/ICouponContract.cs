using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 优惠卷业务接口
    /// </summary>
    public interface ICouponContract : IDependency
    {
        #region Coupon

        Coupon View(int Id);

        CouponDto Edit(int Id);

        IQueryable<Coupon> Coupons { get; }

        IQueryable<CouponItem> CouponItems { get; }

        OperationResult Insert(params CouponDto[] dtos);

        OperationResult Update(params CouponDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Send(int couponId, List<int> listMemberId);

        /// <summary>
        /// 扫描二维码获取优惠券
        /// </summary>
        /// <param name="strNum"></param>
        /// <param name="memberId"></param>
        OperationResult Get(string strNum, int memberId);

        /// <summary>
        /// 将优惠券设为已使用状态
        /// </summary>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        OperationResult SetCouponItemUsed(string couponNum);

        /// <summary>
        /// 将优惠券设为已使用状态
        /// </summary>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        OperationResult SetCouponItemUnUsed(string couponNum);

        #endregion


        /// <summary>
        /// 使用优惠券
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        OperationResult Use(string Number, int MemberId);
    }
}
