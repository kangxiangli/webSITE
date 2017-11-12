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
using System.Linq.Expressions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class CircleService : ServiceBase, ICircleContract
    {
        #region 初始化数据层操作对象

        private readonly IRepository<Circle, int> _circleRepository;

        private readonly IRepository<Member, int> _memberRepository;
        public CircleService(IRepository<Circle, int> circleRepository,
            IRepository<Member, int> memberRepository)
            : base(circleRepository.UnitOfWork)
        {
            _circleRepository = circleRepository;
            _memberRepository = memberRepository;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public Circle View(int Id)
        {
            var entity = _circleRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public CircleDto Edit(int Id)
        {
            var entity = _circleRepository.GetByKey(Id);
            Mapper.CreateMap<Circle, CircleDto>();
            var dto = Mapper.Map<Circle, CircleDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Circle> Circles { get { return _circleRepository.Entities; } }
        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Circle, bool>> predicate, int id = 0)
        {
            return _circleRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params CircleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Circle> listCircle = Circles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listCircle.Where(x => x.CircleName == dto.CircleName).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，名称已经存在");
                    }                    
                }
                OperationResult result = _circleRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params CircleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Circle> listCircle = Circles.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                foreach (var dto in dtos)
                {
                    int count = listCircle.Where(x => x.CircleName == dto.CircleName  && x.Id != dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "编辑失败，名称已经存在");
                    } 
                }
                OperationResult result = _circleRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _circleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _circleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _circleRepository.Entities.Where(m => ids.Contains(m.Id));
                var listCircle = this.Circles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                foreach (var entity in entities)
                {
                    int count = listCircle.Where(x => x.CircleName == entity.CircleName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "恢复失败,和正常的数据名字有重复");
                    }
                    else
                    {
                        entity.IsDeleted = false;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _circleRepository.Update(entity);
                    }
                   
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _circleRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }
        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var listCircle = this.Circles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                var entities = _circleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    int count = listCircle.Where(x => x.CircleName == entity.CircleName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "启用失败,和已经启用的数据名字有重复");
                    }
                    else
                    {
                        entity.IsEnabled = true;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _circleRepository.Update(entity);
                    }                    
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _circleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _circleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据键值对
        /// <summary>
        /// 获取数据键值对
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<Circle> listCircle = Circles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
            if (listCircle.Count() > 0)
            {

                foreach (var Circle in listCircle)
                {
                    list.Add(new SelectListItem() { Text = Circle.CircleName, Value = Circle.Id.ToString() });
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = ""});    
            }            
            return list;
        }
        #endregion

        #region 加入圈子
        /// <summary>
        /// 加入圈子
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="circleId"></param>
        /// <returns></returns>
        public OperationResult AddCircle(int memberId, int circleId)
        {
            try
            {
                Circle circle= this.View(circleId);
                Member member = _memberRepository.GetByKey(memberId);
                if (circle==null || (circle.IsEnabled==false ||circle.IsDeleted==true))
                {
                    return new OperationResult(OperationResultType.Error, "该圈子不存在");
                }
                else
                {
                    int count = circle.Members.Where(x => x.Id == memberId).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "已经加入了该圈子");
                    }
                    else
                    {
                        circle.Members.Add(member);
                        circle.UpdatedTime = DateTime.Now;
                        count = _circleRepository.Update(circle);
                        return count > 0 ? new OperationResult(OperationResultType.Success, "加入成功") : new OperationResult(OperationResultType.Error, "加入失败");
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        #endregion
    }
}
