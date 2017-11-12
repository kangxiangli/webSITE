using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Authorities
{
    public class DepartmentConfiguration : EntityConfigurationBase<Department,int>
    {
       // 2015-9-16
        public DepartmentConfiguration()
        {
            ToTable("A_Department");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
          
        }
    }

   
}