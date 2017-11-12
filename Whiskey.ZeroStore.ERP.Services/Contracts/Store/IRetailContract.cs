using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IRetailContract : IDependency
    {
        IQueryable<Retail> Retails { get; }

        OperationResult Insert(bool isTrans = false, params Retail[] retails);

        OperationResult Update(params Retail[] retails);

        OperationResult Remove(int id);

        OperationResult Recovery(int id);

        OperationResult Enable(int id);

        OperationResult Disable(int id);

        OperationResult Delete(int id);

        DbContextTransaction GetTransaction();



        MemberLoginPassDTO GetMemberInfo(int memberId);
        bool ClearMemberDTOInfo(int memberId);



        /// <summary>
        /// 获取会员可用的优惠活动(等级折扣,店铺活动,优惠券)
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="memberId">会员id</param>
        /// <param name="saleCampIds">已使用的商品活动</param>
        /// <returns></returns>
        OperationResult GetEnableCoupon(int storeId, string memberCard, params int[] saleCampIds);


        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="storeId">店铺Id</param>
        /// <param name="adminId">操作人</param>
        /// <param name="isMember">是否会员</param>
        /// <param name="barcodes">流水号</param>
        /// <returns></returns>
        OperationResult GetProductsInfo(int storeId, int adminId, string memberCard, string[] barcodes, bool isFirstQuery,List<CustomSaleCampsEntry> selectedSaleCamps);

    }
}
