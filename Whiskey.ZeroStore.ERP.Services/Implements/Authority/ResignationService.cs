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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ResignationService : ServiceBase, IResignationContract
    {
        #region 初始化数据层操作对象

        protected readonly IRepository<Resignation, int> _resignationRepository;

        protected readonly IRepository<Administrator, int> _adminRepository;

        public ResignationService(IRepository<Resignation, int> resignationRepository,
            IRepository<Administrator, int> adminRepository)
            : base(resignationRepository.UnitOfWork)
        {
            _resignationRepository = resignationRepository;
            _adminRepository = adminRepository;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public Resignation View(int Id)
        {
            var entity = _resignationRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public ResignationDto Edit(int Id)
        {
            var entity = _resignationRepository.GetByKey(Id);
            Mapper.CreateMap<Resignation, ResignationDto>();
            var dto = Mapper.Map<Resignation, ResignationDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Resignation> Resignations { get { return _resignationRepository.Entities; } }
        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Resignation, bool>> predicate, int id = 0)
        {
            return _resignationRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ResignationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Resignation> listResignation = Resignations.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ToExamineResult != -6 && x.ToExamineResult != -1);
                foreach (ResignationDto dto in dtos)
                {
                    int count = listResignation.Where(x => x.ResignationId == dto.ResignationId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "已经提交离职申请");
                    }
                }
                OperationResult result = _resignationRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.ResignationReason = dto.ResignationReason.Replace("\n", "").Replace("\r\n", "");
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
        public OperationResult Update(params ResignationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Resignation> listResignation = Resignations.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                OperationResult result = _resignationRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.ResignationReason = dto.ResignationReason.Replace("\n", "").Replace("\r\n", "");
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return count > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error, "提交失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        public OperationResult ToExamine(int ToExamineStatues, params int[] ids)
        {
            var enty = _resignationRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.ToExamineResult = ToExamineStatues;
                c.UpdatedTime = DateTime.Now;
            });
            return _resignationRepository.Update(enty.ToList());
        }
    }
}
