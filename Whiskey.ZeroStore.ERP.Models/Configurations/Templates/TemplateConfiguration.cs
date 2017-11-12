using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 模板实体配置
    /// </summary>
    public class TemplateConfiguration : EntityConfigurationBase<Template,int>
    {
        /// <summary>
        /// 构造函数--初始化对象
        /// </summary>
        public TemplateConfiguration()
        {
            //表名称
            ToTable("T_Template");
            //设置主键ID为自增长类型 步长为1
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
