using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 会员搭配业务接口
    /// </summary>
    public interface IMemberColloEleContract : IDependency
    {
        #region IMemberColloEleContract

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        MemberColloEle View(int Id);

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        MemberColloEleDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<MemberColloEle> MemberColloEles { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Insert(params MemberColloEleDto[] dtos);


        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Update(params MemberColloEleDto[] dtos);

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);

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
        /// 删除数据
        /// </summary>
        /// <param name="ids">搭配ID</param>
        /// <returns></returns>
        OperationResult DeleteByMemberColloId(params int[] MemberColloIds);

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
        bool CheckExists(Expression<Func<MemberColloEle, bool>> predicate, int id = 0);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="MemberColloId">搭配Id</param>
        /// <param name="Image">base64string图片</param>
        /// <param name="listColloImage">图片信息集合</param>
        /// <returns></returns>
        OperationResult Insert(int MemberColloId, string Image, List<ImageList> listColloImage, List<TextList> listText);

        #endregion


        void Update(List<MemberColloEle> listEle);
        
    }
}
