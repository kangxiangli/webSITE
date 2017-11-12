using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class HtmlItemConfiguration : EntityConfigurationBase<HtmlItem, int>
    {
        public HtmlItemConfiguration()
        {
            //表名称
            ToTable("T_HtmlItem");
            //设置主键ID为自增长类型 步长为1
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
