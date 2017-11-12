using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 业务层
    /// </summary>
    public class CommentService : ServiceBase, ICommentContract
    {
        #region 声明数据层对象
        private readonly IRepository<Comment, int> _CommentRepository;

        private readonly IRepository<Member, int> _memberRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(CommentService));

        public CommentService(IRepository<Comment, int> CommentRepository,
            IRepository<Member, int> memberRepository)
            : base(CommentRepository.UnitOfWork)
        {
            _CommentRepository = CommentRepository;
            _memberRepository = memberRepository;
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params CommentDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                for (int i = 0; i < dtos.Length; i++)
                {
                    int memberId = dtos[i].MemberId;
                    int index = _memberRepository.Entities.Where(x => x.Id == memberId).Count();
                    if (index == 0)
                    {
                        return new OperationResult(OperationResultType.Error, "会员不存在！");
                    }

                    //禁用对自己的评论进行回复
                    if (dtos[i].ReplyId != 0 && dtos[i].ReplyId != null)
                    {
                        int replyId = (int)dtos[i].ReplyId;
                        var comment = _CommentRepository.Entities.First(x => x.Id == replyId);
                        if (comment.ReplyId.HasValue && comment.MemberId == memberId)
                        {
                            return new OperationResult(OperationResultType.Error, "您无法对自己的评论进行回复！");
                        }
                    }

                }
                OperationResult result = _CommentRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器正在升级维护中，请稍候重试！");
            }
        }
        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _CommentRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取评论列表
        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="singleProId">单品Id</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示量</param>
        /// <returns></returns>
        public IQueryable<Comment> GetList(int sourceId, CommentSourceFlag CommentType, int PageIndex, int PageSize)
        {
            IQueryable<Comment> listComment = _CommentRepository.Entities.Where(x => x.SourceId == sourceId && x.CommentSource == (int)CommentType).OrderBy(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
            return listComment;
        }
        #endregion

        #region 获取单个数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public Comment View(int Id)
        {
            return _CommentRepository.GetByKey(Id);
        }
        #endregion

        #region 获取会员评论
        /// <summary>
        /// 获取会员评论
        /// </summary>
        /// <param name="singleProId">商品Id</param>
        /// <param name="CommentType">评论类型</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数据条数</param>
        /// <returns></returns>
        public List<MemberComment> GetComment(int sourceId, CommentSourceFlag CommentType, int PageIndex, int PageSize)
        {
            IQueryable<Comment> listComment = this.GetList(sourceId, CommentType, PageIndex, PageSize);
            IQueryable<Member> listMember = _memberRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<MemberComment> comments = new List<MemberComment>();
            if (listComment == null)
            {
                return comments;
            }
            else
            {
                comments = (from c in listComment
                            join
                            m in listMember
                            on
                            c.MemberId equals m.Id
                            select new MemberComment
                            {
                                ProductId = c.SourceId,
                                CommentId = c.Id,
                                MemberId = c.MemberId,
                                Content = c.Content,
                                MemberName = m.MemberName,
                                ReplyId = c.ReplyId ?? 0,
                                CommentTime = c.CreatedTime//c.CreatedTime.ToString("yyyy-MM-dd")
                            }).ToList();
                foreach (var item in comments)
                {
                    if (item.ReplyId != 0)
                    {
                        int memberId = this.View(item.ReplyId).MemberId;
                        item.ReplyMemberName = listMember.Where(x => x.Id == memberId).FirstOrDefault().MemberName;
                    }
                }
                return comments;
            }
        }

        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Comment> Comments { get { return _CommentRepository.Entities; } }
        #endregion


        public OperationResult Update(params Comment[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;

                var result = _CommentRepository.Update(entities, s =>
                {
                    s.OperatorId = AuthorityHelper.OperatorId;
                    s.UpdatedTime = DateTime.Now;
                });
                int count = UnitOfWork.SaveChanges();
                result = OperationHelper.ReturnOperationResult(count > 0, opera);
                return result;

            }, Operation.Update);
        }
    }
}
