using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Web.Helper;
using AutoMapper;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Collocation;
using Whiskey.Utility.Helper;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 定义业务层 yky 2015-11-4
    /// </summary>
    public partial class CollocationService : ServiceBase, ICollocationContract
    {
        #region 声明数据层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CollocationService));

        private readonly IRepository<Collocation, int> _CollocationRepository;

        protected readonly IRepository<Member, int> _memberRepository;

        protected readonly IRepository<MissionItem, int> _missionItemRepository;

        protected readonly IRepository<MemberCollRelation, int> _memberCollRelationRepository;

        public CollocationService(IRepository<Collocation, int> CollocationRepository
            ,IRepository<Member, int> memberRepository
            ,IRepository<MissionItem, int> missionItemRepository,
            IRepository<MemberCollRelation, int> memberCollRelationRepository)
            : base(CollocationRepository.UnitOfWork)
        {
            _CollocationRepository = CollocationRepository;
            _memberRepository = memberRepository;
            _missionItemRepository = missionItemRepository;
            _memberCollRelationRepository = memberCollRelationRepository;
        }
        #endregion

        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 获取数据集合
        /// <summary>
        /// 获取数据集合
        /// </summary>
        public IQueryable<Collocation> Collocations
        {
            get { return _CollocationRepository.Entities; }
        }
        #endregion 

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto">领域模型实体</param>
        /// <returns></returns>
        public OperationResult Insert(params CollocationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Collocation> listCollocaton = Collocations;
                for (int i = 0; i < dtos.Length; i++)
                {
                    CollocationDto dto=dtos[i];
                    int count =listCollocaton.Where(x=>x.CollocationName==dto.CollocationName).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "该搭配师的名称已经存在");    
                    }
                }               
                OperationResult result = _CollocationRepository.Insert(dtos, dto =>
				{
					
				},
				(dto, entity) =>
				{
					entity.CreatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;

					return entity;
				});
				return result;
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id">主键Id</param>
        /// <returns></returns>
        public CollocationDto View(int id)
        {             
            Collocation collocation = _CollocationRepository.Entities.Where(x => x.Id == id).FirstOrDefault();                
            Mapper.CreateMap<Collocation, CollocationDto>();
            CollocationDto dto = Mapper.Map<Collocation, CollocationDto>(collocation);
            return dto;            
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto">领域模型实体</param>
        /// <returns></returns>
        public OperationResult Update(params CollocationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Collocation> listCollocations = Collocations;
                for (int i = 0; i < dtos.Length; i++)
                {
                    string name = dtos[i].CollocationName;
                    int id = dtos[i].Id;
                    if (listCollocations.Where(x=>x.CollocationName==name && x.Id!=id).Count()>0)
                    {
                        return new OperationResult(OperationResultType.Error, "该名称已经存在");     
                    }
                }
                                
                OperationResult result = _CollocationRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">主键ID集合</param>
        /// <returns></returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Collocation> listCollocation = Collocations.Where(x=>ids.Contains(x.Id));
                if (listCollocation.Count()>0)
                {
                    foreach (var collocation in listCollocation)
                    {
                        collocation.IsDeleted = true;
                        collocation.UpdatedTime = DateTime.Now;
                        collocation.OperatorId = AuthorityHelper.OperatorId;
                        _CollocationRepository.Update(collocation);
                    }
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "移除失败，数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "移除失败，程序出错");                
            }
        }
        #endregion

        #region 恢复数据
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Collocation> listCollocation = Collocations.Where(x => ids.Contains(x.Id));
                if (listCollocation.Count() > 0)
                {
                    foreach (var collocation in listCollocation)
                    {
                        collocation.IsDeleted = false;
                        collocation.UpdatedTime = DateTime.Now;
                        collocation.OperatorId = AuthorityHelper.OperatorId;
                        _CollocationRepository.Update(collocation);
                    }
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "恢复失败，数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败，程序出错");
            }
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 物理删除数据
        /// </summary>
        /// <param name="ids">主键Id集合</param>
        /// <returns></returns>
        public Utility.Data.OperationResult Delete(params int[] ids)
        {
            try
            {
                var result = _CollocationRepository.Delete(ids);
                return result;
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "程序出错，删除失败");                
            }
        }
        #endregion

        #region 启用数据
        public OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Collocation> listCollocation = Collocations.Where(x => ids.Contains(x.Id));
                foreach (var collocation in listCollocation)
                {
                    collocation.IsEnabled = true;
                    collocation.UpdatedTime = DateTime.Now;
                    collocation.OperatorId = AuthorityHelper.OperatorId;
                    _CollocationRepository.Update(collocation);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "程序出错");                
            }
        }
        #endregion

        #region 禁用数据
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Collocation> listCollocation = Collocations.Where(x => ids.Contains(x.Id));
                foreach (var collocation in listCollocation)
                {
                    collocation.IsEnabled = false;
                    collocation.UpdatedTime = DateTime.Now;
                    collocation.OperatorId = AuthorityHelper.OperatorId;
                    _CollocationRepository.Update(collocation);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "程序出错");
            }
        }
        #endregion

        #region 获取键值对集合
        /// <summary>
        /// 获取键值对集合
        /// </summary>
        /// <param name="title">默认显示值</param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<Collocation> listCollocation = _CollocationRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            if (listCollocation.Count()>0)
            {
                foreach(var collocation in listCollocation)
                {
                    list.Add(new SelectListItem() { Text = collocation.CollocationName, Value = collocation.Id.ToString() });
                }                
            }
            list.Insert(0, new SelectListItem() { Text = title, Value = "-1" });
            return list;
        }
        #endregion


        public OperationResult Insert(params Collocation[] col)
        {
            try
            {
                col.CheckNotNull("coll");

                col.Each(e => { e.CreatedTime = e.UpdatedTime = DateTime.Now; e.OperatorId = AuthorityHelper.OperatorId; if (e.Admini != null) { e.Admini.EntryTime = DateTime.Now; } });
           return _CollocationRepository.Insert((IEnumerable<Collocation>)col) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "新增失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        public OperationResult Update(params Collocation[] colls)
        {
            try
            {
                colls.CheckNotNull("colls");

                colls.Each(e => { e.UpdatedTime = DateTime.Now; e.OperatorId = AuthorityHelper.OperatorId; });
            return _CollocationRepository.Update((ICollection<Collocation>)colls);
        }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        #region 获取搭配师列表
        /// <summary>
        /// 获取搭配师列表
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public OperationResult GetList(int memberId, int PageIndex, int PageSize)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                Member member = _memberRepository.GetByKey(memberId);
                if (member.CollocationId != null)
                {
                    oper.Message = "已经拥有搭配师，无法再次绑定";
                    return oper;
                }
                else
                {
                    IQueryable<Collocation> listCollo = _CollocationRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                    int count = listCollo.Count();
                    int size=count/PageSize;
                    Random random = new Random();
                    PageIndex = random.Next(1,size);
                    listCollo = listCollo.OrderBy(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                    List<string> listNum = new List<string>();
                    foreach (Collocation item in listCollo)
                    {
                        if (!string.IsNullOrEmpty(item.Numb))
                        {
                            listNum.Add(item.Numb);
                        }
                    }                    
                    IQueryable<Member> listMember = _memberRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                    listMember=listMember.Where(x => listNum.Contains(x.UniquelyIdentifies));
                    IQueryable<MissionItem> listMissionItem = _missionItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ScheduleType==(int)ScheduleFlag.Completed);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return oper;
            }
        }
        #endregion

        #region 获取粉丝列表
        /// <summary>
        /// 获取粉丝列表
        /// </summary>
        /// <param name="colloId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public OperationResult GetFansList(int colloId, int PageIndex, int PageSize)
        {
            try
            {
                IQueryable<MemberCollRelation> listCollR= _memberCollRelationRepository.Entities.Where(x => x.CollocationId == colloId && x.IsDeleted == false && x.IsEnabled == true && x.IsUnfriendly == false);
                listCollR = listCollR.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<Member> listMember =  _memberRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var data = (from co in listCollR
                            join
                            me in listMember
                            on
                            co.MemberId equals me.Id
                            select new
                            {
                                FansId = co.MemberId,
                                UserPhoto=strWebUrl+me.UserPhoto,
                                co.CreatedTime,
                            }).ToList();
                return new OperationResult(OperationResultType.Success, "获取成功", data);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            }
        }
        #endregion

     }
}
