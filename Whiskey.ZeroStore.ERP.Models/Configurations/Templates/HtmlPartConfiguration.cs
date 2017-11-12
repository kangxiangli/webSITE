using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models 
{
    class HtmlPartConfiguration: EntityConfigurationBase<HtmlPart, int>
    {
        public HtmlPartConfiguration()
        {             
            ToTable("T_HtmlPart");             
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
