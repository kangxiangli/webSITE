using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PartnerLevelOrderConfiguration : EntityConfigurationBase<PartnerLevelOrder, int>
    {
        public PartnerLevelOrderConfiguration()
        {
            ToTable("O_Partner_Level_Order");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
