using System.Linq;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 优惠卷业务接口
    /// </summary>
    public interface ICouponActivityContract :  IBaseContract<CouponActivity>
    {


        IQueryable<LBSCouponEntity> LBSCouponEntities { get; }

        OperationResult UpdateCoupon(LBSCouponEntity couponEntity);
    
            

    }


  
}
