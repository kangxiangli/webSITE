using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Offices
{
  public  class ClassApplicationConfiguration : EntityConfigurationBase<ClassApplication, int>
    {
        public ClassApplicationConfiguration()
        {
            ToTable("O_ClassApplication");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
