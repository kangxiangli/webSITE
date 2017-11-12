using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 岗位培训blog
    /// </summary>
    public class TrainingBlogEntity : EntityBase<int>
    {
        [DisplayName("标题")]
        [StringLength(maximumLength:100)]
        [Index(IsClustered = false, IsUnique = false)]
        public string Title { get; set; }


        [DisplayName("内容")]
        public string Content { get; set; }

        /// <summary>
        /// 是否培训
        /// </summary>
        [DisplayName("培训")]
        [DefaultValue(false)]
        public bool IsTrain { get; set; }

        /// <summary>
        /// 试卷id
        /// </summary>
        public int? ExamId { get; set; }

        [ForeignKey("ExamId")]
        public virtual ExamEntity Exam { get; set; }

        public virtual Administrator Operator { get; set; }

    }

    public class TrainingBlogEntityConfig : EntityConfigurationBase<TrainingBlogEntity, int>
    {
        public TrainingBlogEntityConfig()
        {
            ToTable("T_TrainingBlog");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }

}
