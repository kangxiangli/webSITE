using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class FieldService : ServiceBase, IFieldContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(FieldService));

        protected readonly IRepository<Field, int> _fieldRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Rest, int> _restRepository;

        private readonly IRepository<Attendance, int> _attendanceRepository;
        private readonly IRepository<Holiday, int> _holidayRepository;
        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaileRepository;
        public FieldService(IRepository<Field, int> fieldRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Rest, int> restRepository,
            IRepository<Attendance, int> attendanceRepository,
            IRepository<WorkTimeDetaile, int> workTimeDetaileRepository,
            IRepository<Holiday, int> holidayRepository)
            : base(fieldRepository.UnitOfWork)
        {
            _fieldRepository = fieldRepository;
            _adminRepository = adminRepository;
            _restRepository = restRepository;
            _attendanceRepository = attendanceRepository;
            _workTimeDetaileRepository = workTimeDetaileRepository;
            _holidayRepository = holidayRepository;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Field View(int Id)
        {
            var entity = _fieldRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public FieldDto Edit(int Id)
        {
            var entity = _fieldRepository.GetByKey(Id);
            Mapper.CreateMap<Field, FieldDto>();
            var dto = Mapper.Map<Field, FieldDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Field> Fields { get { return _fieldRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params FieldDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Field> listField = this.Fields.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                DateTime nowTime = DateTime.Now;
                foreach (FieldDto dto in dtos)
                {
                    IQueryable<Field> listAtt = listField.Where(x => x.AdminId == dto.AdminId);
                    foreach (Field att in listAtt)
                    {
                        if ((att.StartTime <= dto.StartTime && att.EndTime >= dto.StartTime) || (att.StartTime <= dto.EndTime && att.EndTime >= dto.EndTime))
                        {
                            return new OperationResult(OperationResultType.Error, "外勤时间段有重叠，请重新选择时间段！");
                        }
                    }
                }
                OperationResult result = _fieldRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params FieldDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Field> list = this.Fields;
                IQueryable<Field> listField = this.Fields.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                foreach (var dto in dtos)
                {
                    Field field = list.Where(x => x.Id == dto.Id).FirstOrDefault();
                    if (field == null)
                    {
                        return new OperationResult(OperationResultType.Error, "修改失败，数据不存在");
                    }
                    else
                    {
                        if (field.VerifyType == (int)VerifyFlag.Pass)
                        {
                            return new OperationResult(OperationResultType.Error, "审核通过，不能修改数据");
                        }
                        else if (field.VerifyType == (int)VerifyFlag.Verifing)
                        {
                            IQueryable<Field> listAtt = listField.Where(x => x.AdminId == dto.AdminId);
                            foreach (Field att in listAtt)
                            {
                                if ((att.StartTime <= dto.StartTime && att.EndTime >= dto.StartTime) || (att.StartTime <= dto.EndTime && att.EndTime >= dto.EndTime))
                                {
                                    return new OperationResult(OperationResultType.Error, "外勤时间段有重叠，请重新选择时间段！");
                                }
                            }
                        }
                    }
                }

                OperationResult result = _fieldRepository.Update(dtos,
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
                var entities = _fieldRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _fieldRepository.Update(entity);
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
                var entities = _fieldRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _fieldRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
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
                var entities = _fieldRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _fieldRepository.Update(entity);
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
                var entities = _fieldRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _fieldRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 审核数据
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Verify(FieldDto dto)
        {
            try
            {
                int adminId = dto.AdminId;
                OperationResult oper = new OperationResult(OperationResultType.Error, "审核数据不存在");
                Field field = _fieldRepository.Entities.Where(x => x.AdminId == adminId && x.Id == dto.Id).FirstOrDefault();
                if (field == null)
                {
                    return oper;
                }
                else
                {
                    if (field.VerifyType == (int)VerifyFlag.Pass)
                    {
                        if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            oper = VerifyNoPass(field);
                            return oper;
                        }
                    }
                    else
                    {
                        if (dto.VerifyType == (int)VerifyFlag.Pass)
                        {
                            oper = VerifyPass(field, adminId);
                            return oper;
                        }
                        else
                        {
                            oper = VerifyNoPass(field);
                            return oper;
                        }
                    }
                    return oper;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问");
            }
        }
        #endregion

        #region 审核不通过
        /// <summary>
        /// 审核不通过
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private OperationResult VerifyNoPass(Field field)
        {
            field.VerifyType = (int)VerifyFlag.NoPass;
            field.UpdatedTime = DateTime.Now;
            field.OperatorId = AuthorityHelper.OperatorId;
            int num = _fieldRepository.Update(field);
            return num > 0 ? new OperationResult(OperationResultType.Success, "审核成功") : new OperationResult(OperationResultType.Error, "审核失败");
        }
        #endregion

        #region 审核通过
        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="field"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult VerifyPass(Field field, int adminId)
        {
            Administrator admin = _adminRepository.Entities.Where(x => x.Id == adminId).FirstOrDefault();
            if (admin == null) return new OperationResult(OperationResultType.Error, "审核的员工不存在");
            if (admin.JobPosition == null) return new OperationResult(OperationResultType.Error, "该员工没有工作职位，添加数据失败");
            field.VerifyType = (int)VerifyFlag.Pass;
            field.UpdatedTime = DateTime.Now;
            field.OperatorId = AuthorityHelper.OperatorId;
            int num = _fieldRepository.Update(field);
            return num > 0 ? new OperationResult(OperationResultType.Success, "审核成功") : new OperationResult(OperationResultType.Error, "审核失败");
        }
        #endregion
    }
}
