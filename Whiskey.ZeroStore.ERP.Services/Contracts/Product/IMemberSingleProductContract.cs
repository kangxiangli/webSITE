using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.ProductInfo;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberSingleProductContract : IDependency
    {
        #region IMemberSingleProductContract

        MemberSingleProduct View(int Id);

        MemberSingleProductDto Edit(int Id);

        IQueryable<MemberSingleProduct> MemberSingleProducts { get; }

        OperationResult Insert(params MemberSingleProductDto[] dtos);

        OperationResult Update(params MemberSingleProductDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="id">标识Id</param>
        /// <returns></returns>
        OperationResult Delete(int memberId,int id);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        IEnumerable<SelectListItem> SelectList(string title);

        bool CheckExists(Expression<Func<MemberSingleProduct, bool>> predicate, int id = 0);
        
        /// <summary>
        /// 获取单品集合
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数据量</param>
        void GetList(int memberId, int PageIndex, int PageSize);

        List<MemberProductInfo> GetAllList(SingleProductFlag? flag,int memberId, int? strColorId, int? strProductAttrId, int? strCategoryId, int PageIndex, int PageSize);

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MemberProductInfo GetEdit(int id);

         #endregion









        

        void Update(List<MemberSingleProduct> list);

        string GetAttrNames(string attrIds);
       
    }
}
