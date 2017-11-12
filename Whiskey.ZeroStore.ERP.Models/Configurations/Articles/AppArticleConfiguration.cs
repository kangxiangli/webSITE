using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AppArticleConfiguration : EntityConfigurationBase<AppArticle, int>
    {
        public AppArticleConfiguration()
        {
            ToTable("P_App_Article");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
