
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class CollocationQuestionnaireConfiguration : EntityConfigurationBase<CollocationQuestionnaire, int>
    {
        public CollocationQuestionnaireConfiguration()
        {
            ToTable("M_CollocationQuestionnaire");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

