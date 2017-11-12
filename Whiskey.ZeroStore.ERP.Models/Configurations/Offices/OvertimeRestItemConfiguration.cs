using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;


namespace Whiskey.ZeroStore.ERP.Models
{
    public class OvertimeRestItemConfiguration: EntityConfigurationBase<OvertimeRestItem, int>
    {
        public OvertimeRestItemConfiguration()
        {
            ToTable("O_Overtime_Rest_Item");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
