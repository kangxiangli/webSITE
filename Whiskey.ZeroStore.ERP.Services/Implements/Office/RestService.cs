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
using Whiskey.Utility.Logging;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class RestService : ServiceBase, IRestContract
    {

        #region 声明数据层操作对象

        private readonly IRepository<Rest, int> _restRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(RestService));

        public RestService(IRepository<Rest, int> restRepository,
            IRepository<Administrator, int> adminRepository)
            : base(restRepository.UnitOfWork)
        {
            _restRepository = restRepository;
            _adminRepository = adminRepository;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Rest View(int Id)
        {
            var entity = _restRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public RestDto Edit(int Id)
        {
            var entity = _restRepository.GetByKey(Id);
            Mapper.CreateMap<Rest, RestDto>();
            var dto = Mapper.Map<Rest, RestDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Rest> Rests { get { return _restRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params RestDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");

                OperationResult result = _restRepository.Insert(dtos,
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

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params RestDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _restRepository.Update(dtos,
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

        public OperationResult Update(Rest rest)
        {
            int count = _restRepository.Update(rest);
            if (count > 0)
            {
                return new OperationResult(OperationResultType.Success, "添加成功");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "添加失败");
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
                var entities = _restRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _restRepository.Update(entity);
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
                var entities = _restRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _restRepository.Update(entity);
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
                var entities = _restRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _restRepository.Update(entity);
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
                var entities = _restRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _restRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 重置年假
        public OperationResult Reset(int? departmentId = 0)
        {
            try
            {

                IQueryable<Administrator> listAdmin = departmentId == null || departmentId == 0 ? _adminRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true) : _adminRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == departmentId);
                DateTime nowTime = DateTime.Now;
                UnitOfWork.TransactionEnabled = true;
                foreach (Administrator admin in listAdmin)
                {
                    if (admin.JobPosition == null)
                    {
                        continue;
                    }
                    #region 当员工有工作职位时
                    AnnualLeave annualLeave = admin.JobPosition.AnnualLeave;
                    if (annualLeave.Children == null)
                    {
                        return new OperationResult(OperationResultType.Error, "请添加年假的详情");
                    }
                    List<AnnualLeave> listAnnualLeave = annualLeave.Children.ToList();
                    TimeSpan timeSpan = nowTime - admin.CreatedTime;
                    double year = Math.Round(timeSpan.TotalDays / 365, MidpointRounding.AwayFromZero);
                    AnnualLeave ann = listAnnualLeave.Where(x => x.StartYear <= year && x.EndYear > year).FirstOrDefault();
                    if (ann == null)
                    {
                        return new OperationResult(OperationResultType.Error, "年假范围出错");
                    }
                    Rest rest = _restRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == admin.Id).FirstOrDefault();
                    if (rest == null)
                    {
                        rest = new Rest()
                        {
                            AdminId = admin.Id,
                            AnnualLeaveDays = ann.Days,
                            CreatedTime = nowTime,
                            UpdatedTime = nowTime
                        };
                        _restRepository.Insert(rest);
                    }
                    else
                    {
                        rest.AnnualLeaveDays = ann.Days;
                        rest.UpdatedTime = nowTime;
                        _restRepository.Update(rest);
                    }
                    #endregion
                }
                int count = UnitOfWork.SaveChanges();
                return count > 0 ? new OperationResult(OperationResultType.Success, "重置成功") : new OperationResult(OperationResultType.Error, "重置失败");
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
