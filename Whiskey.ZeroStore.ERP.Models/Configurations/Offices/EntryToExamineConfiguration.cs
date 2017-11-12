using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Offices
{
    public class EntryToExamineConfiguration : EntityConfigurationBase<EntryToExamine, int>
    {
        public EntryToExamineConfiguration()
        {
            ToTable("O_EntryToExamine");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
