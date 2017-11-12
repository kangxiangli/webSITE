




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
    public class GalleryConfiguration : EntityConfigurationBase<Gallery, int>
    {
        public GalleryConfiguration() {
            ToTable("G_Gallery");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             

            HasMany(m => m.Colors).WithMany(m => m.Galleries).Map(m =>
            {
                m.ToTable("G_Gallery_Color_Relation");
                m.MapLeftKey("GalleryId");
                m.MapRightKey("ColorId");
            });

            HasMany(m => m.GalleryAttributes).WithMany(m => m.Galleries).Map(m =>
            {
                m.ToTable("G_Gallery_Attribute_Relation");
                m.MapLeftKey("GalleryId");
                m.MapRightKey("GalleryAttributeId");
            });
        }
    }
}
