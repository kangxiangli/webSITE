using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class TopicImageConfiguration : EntityConfigurationBase<TopicImage, int>
    {
        public TopicImageConfiguration()
        {
            ToTable("M_Topic_Image");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }                          
    }
}
