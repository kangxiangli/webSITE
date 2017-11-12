using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 会员搭配业务接口
    /// </summary>
    public interface IMemberCollocationContract : IDependency
    {
        #region IMemberCollocationContract

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        MemberCollocation View(int Id);

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        MemberCollocationDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<MemberCollocation> MemberCollocations { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Insert(params MemberCollocationDto[] dtos);
        
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Update(params MemberCollocationDto[] dtos);

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);

        OperationResult Remove(int MemberId, params int[] ids);

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Recovery(params int[] ids);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">ID</param>
        /// <returns></returns>
        OperationResult Enable(params int[] ids);

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">ID</param>
        /// <returns></returns>
        OperationResult Disable(params int[] ids);

        /// <summary>
        /// 根据条件校验数据是否存在
        /// </summary>
        /// <param name="predicate">Lamda表达式</param>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        bool CheckExists(Expression<Func<MemberCollocation, bool>> predicate, int id = 0);
        

        ///// <summary>
        ///// 获取数据集
        ///// </summary>
        ///// <param name="memberId">会员Id</param>
        ///// <param name="strColloName">搭配名称</param>
        ///// <param name="strColor">颜色</param>
        ///// <param name="strSeason">季节</param>
        ///// <param name="strStyle">风格</param>
        ///// <param name="strSituation">场合</param>
        ///// <param name="operRes">操作信息</param>
        //void GetList(int memberId, string strColloName, string strColor, string strSeason, string strStyle, string strSituation, out OperationResult operRes);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="memberId">会员Id</param>        
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示数据条数</param>
        /// <param name="operRes">操作信息</param>
        List<MemberCollo> GetList(int memberId, string strColloName, string strColorId, string strSeasonId, string strProductAttrId, string strSituationId, int PageIndex, int PageSize, out PagedOperationResult operRes);

        /// <summary>
        /// 批量获取会员搭配信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<MemberCollo> GetList(int memberId);

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="colloId">搭配Id</param>
        /// <param name="operRest">操作信息</param>
        /// <returns></returns>
        OperationResult GetEdit(int memberId, int colloId);

        /// <summary>
        /// 获取搭配详情
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="colloId">搭配Id</param>
        OperationResult  GetDeail(int memberId, int colloId);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="myColl"></param>
        /// <returns></returns>
        OperationResult Add(MyColl myColl);
        /// <summary>
        /// 保存编辑数据
        /// </summary>
        /// <param name="colloInfo"></param>
        OperationResult SaveEdit(ColloInfo colloInfo);

        OperationResult Recommend(int[] Id);

        OperationResult CancleRecommend(int[] Id);

        #endregion



        OperationResult SaveRecommendMemberId(int id, params int[] recommendMemberIds);

        OperationResult SaveRecommendCollocationId(int memberId, params int[] recommendCollocationIds);
    }
}
