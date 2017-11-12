using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations
{
    public class CallthepoliceConfiguration : EntityConfigurationBase<Callthepolice, int>
    {
        public CallthepoliceConfiguration()
        {
            ToTable("V_Callthepolice");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
