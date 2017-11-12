




using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体配置
    /// </summary>
    public class SubjectConfiguration : EntityConfigurationBase<Subject, int>
    {
        public SubjectConfiguration() {
            ToTable("S_Subject");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasMany(m => m.SubjectAttributes).WithMany(m => m.Subjects).Map(m =>
            {
                m.ToTable("S_Subject_Attribute_Relation");
                m.MapLeftKey("SubjectId");
                m.MapRightKey("AttributeId");
            });

        }
    }
}
