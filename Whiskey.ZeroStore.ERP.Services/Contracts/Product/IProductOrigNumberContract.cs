using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IProductOrigNumberContract : IDependency
    {
        IQueryable<ProductOriginNumber> OrigNumbs { get; }

        OperationResult Insert(params ProductOriginNumber[] produarr);

        ProductOriginNumber View(int Id);

        OperationResult Update(params ProductOriginNumber[] orignum);
        /// <summary>
        /// 更新原始货号的商品价格
        /// </summary>
        /// <returns></returns>
        OperationResult UpdatePriceByDiscount(ProductDiscount discount, int[] origIds, bool isTrans);
        OperationResult Remove(bool isTrans, params int[] ids);

        OperationResult Delete(bool isTrans, int[] ints);

        bool isHave(string Id);

        string GetOrigNumId(string SampleId);

        OperationResult SetRecommand(bool IsRecommend, params string[] bigProNums);
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Verify(params int[] ids);
        /// <summary>
        /// 审核拒绝
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Reason"></param>
        /// <returns></returns>
        OperationResult VerifyRefuse(int Id, string RefuseReason);
    }
}
