using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 定义业务操作接口
    /// </summary>
    public interface ITemplateContract : IDependency
    {
        #region Template
         

        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        TemplateDto Edit(int Id);
        /// <summary>
        /// 获取数据对象
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        Template View(int Id);

        IQueryable<Template> Templates { get; }

        OperationResult Update(params TemplateDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        /// <summary>
        /// 校验模版名称
        /// </summary>
        /// <param name="templateName">模版名称</param>
        /// <returns></returns>
        int CheckTemplateName(string templateName, TemplateFlag templateSort);

        /// <summary>
        /// 获取模版键值对
        /// </summary>
        /// <param name="title">默认显示值</param>
        /// <returns></returns>
        IEnumerable<KeyValue<string, string>> SelectList(string title);

        List<SelectListItem> SelectList(string title, TemplateFlag templateFlag);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>        
        /// <returns></returns>
        OperationResult Insert(params TemplateDto[] dtos);

        /// <summary>
        /// 设为默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="templateFlag"></param>
        /// <returns></returns>
        OperationResult SetDefault(int Id, TemplateFlag templateFlag, TemplateTypeFlag templateTypeFlag = TemplateTypeFlag.PC);

        /// <summary>
        /// 获取默认模板通知的模板信息
        /// </summary>
        /// <param name="templateNotiFlag"></param>
        /// <param name="tempflag"></param>
        /// <returns></returns>
        Template GetNotificationTemplate(TemplateNotificationType templateNotiFlag, TemplateFlag tempflag = TemplateFlag.Notification);

        OperationResult Build(params int[] Id);

        OperationResult SetIndex(int Id);
        #endregion

        /// <summary>
        /// 获取通知模板下未开启EnabledPerNotifi的所有选择部门类型下的所有部门Ids
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        List<int> GetNotificationDepartIds(int Id);

    }
}
