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
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class HolidayService : ServiceBase, IHolidayContract
    {

        #region 声明数据层操作对象
        
        private readonly IRepository<Holiday, int> _holidayRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(HolidayService));

        public HolidayService(IRepository<Holiday, int> holidayRepository,
            IRepository<Administrator, int> adminRepository)
            : base(holidayRepository.UnitOfWork)
		{
            _holidayRepository = holidayRepository;
            _adminRepository = adminRepository;
		}
        #endregion

        protected string _holidayKey { get { return OfficeHelper._holidayKey; } }

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Holiday View(int Id)
        {
            var entity = _holidayRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public HolidayDto Edit(int Id)
        {
            var entity = _holidayRepository.GetByKey(Id);
            Mapper.CreateMap<Holiday, HolidayDto>();
            var dto = Mapper.Map<Holiday, HolidayDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Holiday> Holidays { get { return _holidayRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params HolidayDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Holiday> list = this.Holidays.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                List<string> listDates = new List<string>();
                if (list.Count() > 0)
                {
                    listDates = list.Select(x => x.WorkDates).Where(x =>!string.IsNullOrEmpty(x)).ToList();
                }
                foreach (HolidayDto dto in dtos)
                {
                    int count = list.Where(x => x.HolidayName == dto.HolidayName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "名字重复");
                    }
                    count = list.Where(x => x.StartTime.CompareTo(dto.StartTime) >= 0 && x.EndTime.CompareTo(dto.EndTime) <= 0).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "休假日期重复");
                    }
                    if (!string.IsNullOrEmpty(dto.WorkDates))
                    {
                        var arrDates = dto.WorkDates.Split(',');
                        foreach (string date in arrDates)
                        {
                            if (!string.IsNullOrEmpty(date))
                            {
                                count = list.Where(x => x.WorkDates.Contains(date)).Count();
                                if (count > 0)
                                {
                                    return new OperationResult(OperationResultType.Error, "工作日期重复");
                                }
                                DateTime currentDate = DateTime.Parse(date);
                                count = list.Where(x => x.StartTime.CompareTo(currentDate) >= 0 && x.EndTime.CompareTo(currentDate) <= 0).Count();
                                if (count > 0)
                                {
                                    return new OperationResult(OperationResultType.Error, "工作日期中含有休假日期");
                                }
                            }
                        }                        
                    }
                    var oper =this.CheckDate(list, dto.StartTime, dto.EndTime);
                    if (oper.ResultType!=OperationResultType.Success)
                    {
                        return oper;
                    }
                }
                OperationResult result = _holidayRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });        
                //添加成功后，将数据重新放到缓存中        
                if (result.ResultType==OperationResultType.Success)
                {
                    this.Refresh();
                }
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
        public OperationResult Update(params HolidayDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Holiday> list = this.Holidays.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                List<string> listDates = new List<string>();
                if (list.Count()>0)
                {
                    listDates = list.Select(x => x.WorkDates).Where(x => !string.IsNullOrEmpty(x)).ToList();
                }
                foreach (HolidayDto dto in dtos)
                {
                    int count = list.Where(x => x.HolidayName == dto.HolidayName && x.Id!=dto.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名字重复");
                    }
                    count = list.Where(x => x.StartTime.CompareTo(dto.StartTime) >= 0 && x.EndTime.CompareTo(dto.EndTime) <= 0 && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "休假日期重复");
                    }
                    if (!string.IsNullOrEmpty(dto.WorkDates))
                    {
                        var arrDates = dto.WorkDates.Split(',');
                        foreach (string date in arrDates)
                        {
                            if (!string.IsNullOrEmpty(date))
                            {
                                count = list.Where(x => x.WorkDates.Contains(date) && x.Id != dto.Id).Count();
                                if (count > 0)
                                {
                                    return new OperationResult(OperationResultType.Error, "工作日期重复");
                                }
                                DateTime currentDate = DateTime.Parse(date);
                                count = list.Where(x => x.StartTime.CompareTo(currentDate) >= 0 && x.EndTime.CompareTo(currentDate) <= 0).Count();
                                if (count > 0)
                                {
                                    return new OperationResult(OperationResultType.Error, "工作日期中含有休假日期");
                                }
                            }
                        }
                    }
                    var oper = this.CheckDate(list, dto.StartTime, dto.EndTime);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return oper;
                    }
                }
                OperationResult result = _holidayRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                //提交成功后，将数据重新放到缓存中        
                if (result.ResultType == OperationResultType.Success)
                {
                    this.Refresh();
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        
        #endregion

        #region 校验休假日期和工作日期是否冲突
        private OperationResult CheckDate(IQueryable<Holiday> list,DateTime startTime,DateTime endTime)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            List<string> listDates = new List<string>();
            if (list.Count() > 0)
            {
                listDates = list.Select(x => x.WorkDates).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }            
            if (listDates.Count() > 0)
            {
                foreach (string date in listDates)
                {
                    string[] arrDate = date.Split(",");
                    foreach (string dateTime in arrDate)
                    {
                        if (!string.IsNullOrEmpty(dateTime))
                        {
                            DateTime currentDate = DateTime.Parse(dateTime);
                            if (startTime.CompareTo(currentDate) >= 0 && endTime.CompareTo(currentDate) <= 0)
                            {
                                return new OperationResult(OperationResultType.Error, "休假日期中含有工作日期");
                            }
                        }
                    }
                    
                }
            }
            return oper;
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
                var entities = _holidayRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _holidayRepository.Update(entity);
                }
                int count = UnitOfWork.SaveChanges();
                //提交成功后，将数据重新放到缓存中        
                if (count>0)
                {
                    this.Refresh();
                }
                return count > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
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
                var entities = _holidayRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _holidayRepository.Update(entity);
                }
                int count = UnitOfWork.SaveChanges();
                //提交成功后，将数据重新放到缓存中        
                if (count > 0)
                {
                    this.Refresh();
                }
                return count > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
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
                var entities = _holidayRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _holidayRepository.Update(entity);
                }
                int count = UnitOfWork.SaveChanges();
                //提交成功后，将数据重新放到缓存中        
                if (count > 0)
                {
                    this.Refresh();
                }
                return count > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
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
                var entities = _holidayRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _holidayRepository.Update(entity);
                }
                int count = UnitOfWork.SaveChanges();
                //提交成功后，将数据重新放到缓存中        
                if (count > 0)
                {                    
                    this.Refresh();
                }
                return count > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取节假日
        public Dictionary<string, int> GetHoliday()
        {
            object obj = CacheHelper.GetCache(_holidayKey);
            Dictionary<string, int> dic= new Dictionary<string,int>();
            if (obj==null)
            {
                List<Holiday> list = this.Holidays.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                dic = OfficeHelper.InsertHolidayCache(list);
            }
            else
            {
                dic = CacheHelper.GetCache(_holidayKey) as Dictionary<string, int>;
            }
            return dic;
        }
        #endregion
 
        #region 重新装载数据到缓存中
        private void Refresh()
        {
            CacheHelper.RemoveCache(_holidayKey);
            this.GetHoliday();
        }
        #endregion
    }
}
