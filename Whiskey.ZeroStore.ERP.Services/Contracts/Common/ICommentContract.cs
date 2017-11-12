using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ICommentContract : IDependency
    {
        #region 业务接口
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params CommentDto[] dtos);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        OperationResult Update(params Comment[] entities);

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="singleProId">单品Id</param>
        /// <param name="productCommentType">评论类型</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示量</param>
        /// <returns></returns>
        IQueryable<Comment> GetList(int sourceId, CommentSourceFlag CommentType, int PageIndex, int PageSize);

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="nullable"></param>
        /// <returns></returns>
        Comment View(int Id);

         

        /// <summary>
        /// 获取会员评论
        /// </summary>
        /// <param name="singleProId">商品Id</param>
        /// <param name="productCommentType">评论类型</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数据条数</param>
        /// <returns></returns>
        List<MemberComment> GetComment(int sourceId, CommentSourceFlag CommentType, int PageIndex, int PageSize);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<Comment> Comments { get; }
        #endregion





    }
}
