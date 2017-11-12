using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Video
{
   public class VideoJurisdictionConfiguration : EntityConfigurationBase<VideoJurisdiction, int>
    {
        public VideoJurisdictionConfiguration()
        {
            ToTable("V_VideoJurisdiction");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
