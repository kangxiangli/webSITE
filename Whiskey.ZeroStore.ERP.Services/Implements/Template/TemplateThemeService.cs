using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using AutoMapper;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class TemplateThemeService : ServiceBase, ITemplateThemeContract
    {
        private readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateThemeService));

        private readonly IRepository<TemplateTheme, int> _TemplateThemeRepository;
        public TemplateThemeService(IRepository<TemplateTheme, int> _TemplateThemeRepository)
            : base(_TemplateThemeRepository.UnitOfWork)
        {
            this._TemplateThemeRepository = _TemplateThemeRepository;
        }

        public OperationResult Update(params TemplateThemeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _TemplateThemeRepository.Update(dtos,
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

        public IQueryable<TemplateTheme> templateThemes
        {
            get { return _TemplateThemeRepository.Entities; }
        }

        public OperationResult Insert(params TemplateThemeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _TemplateThemeRepository.Insert(dtos,
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

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                return _TemplateThemeRepository.Delete(ids);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }
        }

        public TemplateTheme View(int id)
        {
            return _TemplateThemeRepository.GetByKey(id);
        }

        public TemplateThemeDto Edit(int id)
        {
            TemplateTheme template = _TemplateThemeRepository.GetByKey(id);
            //Mapper.CreateMap<Template, TemplateThemeDto>();
            var dto = Mapper.Map<TemplateTheme, TemplateThemeDto>(template);
            return dto;
        }

        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _TemplateThemeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_TemplateThemeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }

        public OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _TemplateThemeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_TemplateThemeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }

        #region 设为默认模板
        /// <summary>
        /// 设为默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="templateFlag"></param>
        /// <returns></returns>
        public OperationResult SetDefault(int Id)
        {
            try
            {
                List<TemplateThemeDto> listDto = new List<TemplateThemeDto>();
                IQueryable<TemplateTheme> listEntity = this._TemplateThemeRepository.Entities;
                TemplateTheme temp = listEntity.Where(x => x.Id == Id).FirstOrDefault();
                if (temp != null)
                {
                    var dto = Mapper.Map<TemplateTheme, TemplateThemeDto>(temp);
                    dto.IsDefault = true;
                    listDto.Add(dto);
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "模板不存在");
                }
                IQueryable<TemplateTheme> listDefault = listEntity.Where(x => x.IsDefault == true && x.ThemeFlag == temp.ThemeFlag && x.Id != temp.Id);
                foreach (var item in listDefault)
                {
                    var dto = Mapper.Map<TemplateTheme, TemplateThemeDto>(item);
                    dto.IsDefault = false;
                    listDto.Add(dto);
                }
                OperationResult oper = this.Update(listDto.ToArray());
                return oper;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "设置失败！");
            }
        }
        #endregion

        #region 校验名称是否已存在
        /// <summary>
        /// 校验模版名称
        /// </summary>
        /// <param name="templateName">模版名称</param>
        /// <returns>True表示已存在</returns>
        public bool CheckTemplateName(string themeName, int? Id)
        {
            if (string.IsNullOrEmpty(themeName)) return false;
            return _TemplateThemeRepository.Entities.Count(x => x.Name == themeName && x.Id != Id) > 0;
        }

        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">主键Id</param>
        /// <returns></returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _TemplateThemeRepository.Entities.Where(m => ids.Where(k => k == m.Id).Count() > 0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_TemplateThemeRepository.Update(entity);
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
        /// <param name="ids">主键Id</param>
        /// <returns></returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _TemplateThemeRepository.Entities.Where(m => ids.Where(k => k == m.Id).Count() > 0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_TemplateThemeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion 
    }
}
