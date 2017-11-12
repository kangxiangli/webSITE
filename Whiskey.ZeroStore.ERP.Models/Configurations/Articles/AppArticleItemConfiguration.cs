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
    public class AppArticleItemConfiguration : EntityConfigurationBase<AppArticleItem, int>
    {
        public AppArticleItemConfiguration()
        {
            ToTable("P_App_ArticleItem");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
