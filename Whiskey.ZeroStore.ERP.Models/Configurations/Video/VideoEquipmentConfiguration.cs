using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Video
{
   public class VideoEquipmentConfiguration : EntityConfigurationBase<VideoEquipment, int>
    {
        public VideoEquipmentConfiguration()
        {
            ToTable("V_VideoEquipment");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
