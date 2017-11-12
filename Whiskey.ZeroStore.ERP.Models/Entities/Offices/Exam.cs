using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 试卷
    /// </summary>
    public class ExamEntity : EntityBase<int>
    {
        public ExamEntity()
        {
            Questions = new List<ExamQuestionEntity>();
        }
        [DisplayName("试卷名称")]
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 满分
        /// </summary>
        [DisplayName("满分")]
        [Range(0, 9999)]
        [RegularExpression(@"\d+", ErrorMessage = "{0}必须是整数")]

        public int TotalScore { get; set; }

        /// <summary>
        /// 及格线
        /// </summary>
        [DisplayName("及格线")]
        [Range(0, 100)]
        [RegularExpression(@"\d+", ErrorMessage = "{0}必须是整数")]

        public int PassLine { get; set; }


        /// <summary>
        /// 题目数量
        /// </summary>
        [DisplayName("题目数量")]
        public int QuestionCount { get; set; }


        ///// <summary>
        ///// 考试总人数
        ///// </summary>
        //[DisplayName("考试总人数")]
        //public int TakeExamCount { get; set; }


        /// <summary>
        /// 重考时消耗的积分
        /// </summary>
        [DisplayName("重考时消耗的积分")]
        [RegularExpression(@"\d+", ErrorMessage = "{0}必须是整数")]
        public int RetryCostScore { get; set; }


        /// <summary>
        /// 答题时间,单位:分钟
        /// </summary>
        [DisplayName("答题时间,单位:分钟")]
        [RegularExpression(@"\d+", ErrorMessage = "{0}必须是整数")]
        [Range(1, 999)]
        public int MinutesLimit { get; set; }



        /// <summary>
        /// 奖励积分
        /// </summary>
        [DisplayName("奖励积分")]
        [Range(0, 9999)]
        public int? RewardMemberScore { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<ExamQuestionEntity> Questions { get; set; }

    }


    /// <summary>
    /// 考试记录
    /// </summary>
    public class ExamRecordEntity : EntityBase<int>
    {

        /// <summary>
        /// 试卷id
        /// </summary>
        public int ExamId { get; set; }


        /// <summary>
        /// blogid
        /// </summary>
        public int TraingBlogId { get; set; }

        [ForeignKey("ExamId")]
        public virtual ExamEntity Exam { get; set; }

        [ForeignKey("TraingBlogId")]
        public virtual TrainingBlogEntity TrainingBlog { get; set; }
        /// <summary>
        /// 员工id
        /// </summary>
        public int AdminId { get; set; }
        [ForeignKey("AdminId ")]
        public virtual Administrator Admin { get; set; }


        /// <summary>
        /// 状态
        /// </summary>
        [DisplayName("状态")]
        [Required]

        public ExamRecordStateEnum? State { get; set; }


        /// <summary>
        /// 开始答题的时间点,用于在提交试卷时判断是否超时
        /// </summary>
        [DisplayName("开始答题的时间点")]
        public DateTime? StartTimePoint { get; set; }



        /// <summary>
        /// 提交时间
        /// </summary>
        [DisplayName("提交时间")]
        public DateTime? SubmitTimePoint { get; set; }

        /// <summary>
        /// 答题详情
        /// </summary>
        [DisplayName("答题详情")]
        public string AnswerDetail { get; set; }

        /// <summary>
        /// 试卷得分
        /// </summary>
        [DisplayName("试卷得分")]
        public int GetScore { get; set; }


        /// <summary>
        /// 奖励积分
        /// </summary>
        [DisplayName("奖励积分")]
        public int? RewardMemberScore { get; set; }

        /// <summary>
        /// 是否通过(及格)
        /// </summary>
        public bool IsPass { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 入职培训状态（null：非入职培训；  0：未答题；  1：已接受）
        /// </summary>
        [Display(Name = "入职培训状态")]
        public int? EntryTrainStatus { get; set; }
    }


    /// <summary>
    /// 答题状态
    /// </summary>
    public enum ExamRecordStateEnum
    {
        未开始 = 0,
        答题中 = 1,
        已提交 = 2
    }


    /// <summary>
    /// 试题
    /// </summary>
    public class ExamQuestionEntity : EntityBase<int>
    {
        /// <summary>
        /// 标题
        /// </summary>
        [DisplayName("标题")]
        [Required]
        public string Title { get; set; }


        [DisplayName("图片")]
        public string ImgUrl { get; set; }


        /// <summary>
        /// 是否多选
        /// </summary>
        [DisplayName("是否多选")]
        [Required]
        public bool? IsMulti { get; set; }


        /// <summary>
        /// 答案
        /// </summary>
        [DisplayName("正确答案")]
        [Required]
        public string RightAnswer { get; set; }


        /// <summary>
        /// 分值
        /// </summary>
        [DisplayName("分值")]
        public int Score { get; set; }

        /// <summary>
        /// 选择项
        /// </summary>
        [Required]
        [DisplayName("选择项")]
        public string AnswerOptions { get; set; }




        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<ExamEntity> Exams { get; set; }

    }

    /// <summary>
    /// 试题展示dto
    /// </summary>
    public class ExamQuestionDTO
    {
        public int Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [DisplayName("标题")]
        public string Title { get; set; }


        /// <summary>
        /// 图片
        /// </summary>
        [DisplayName("图片")]
        public string ImgUrl { get; set; }



        /// <summary>
        /// 是否多选
        /// </summary>
        [DisplayName("是否多选")]
        public bool IsMulti { get; set; }


        /// <summary>
        /// 答案
        /// </summary>
        [DisplayName("正确答案")]
        public string[] RightAnswer { get; set; }


        /// <summary>
        /// 分值
        /// </summary>
        [DisplayName("分值")]
        public int Score { get; set; }

        /// <summary>
        /// 选择项
        /// </summary>
        [DisplayName("选择项")]
        public List<AnswerOptionEntry> AnswerOptions { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }


    }

    /// <summary>
    /// 试卷dto
    /// </summary>
    public class ExamEntityConfig : EntityConfigurationBase<ExamEntity, int>
    {
        public ExamEntityConfig()
        {

            ToTable("E_Exam");
            Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(e => e.Questions).WithMany(q => q.Exams)
            .Map(config => config.MapLeftKey("ExamId").MapRightKey("ExamQuestionId"));
        }
    }

    /// <summary>
    /// 问题
    /// </summary>
    public class ExamQuestionEntityConfig : EntityConfigurationBase<ExamQuestionEntity, int>
    {
        public ExamQuestionEntityConfig()
        {
            ToTable("E_ExamQuestion");
            Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
    public class ExamRecordEntityConfig : EntityConfigurationBase<ExamRecordEntity, int>
    {
        public ExamRecordEntityConfig()
        {
            ToTable("E_ExamRecord");
        }
    }


    /// <summary>
    /// 答案dto
    /// </summary>
    public class AnswerOptionEntry
    {

        /// <summary>
        /// 答案内容
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 答案值A,B,C,D顺序表示
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// 答案的图片
        /// </summary>
        public string ImgUrl { get; set; }



        /// <summary>
        /// 是否被勾选(答题详情时用于展示勾选的答案)
        /// </summary>
        public bool IsChecked { get; set; }

    }




    public class tempDTO
    {
        public tempDTO()
        {

        }
        public int ExamRecordId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int ExamTotalScore { get; set; }
        public int ExamQuestionCount { get; set; }
        public int ExamMinutesLimit { get; set; }

        public int GetScore { get; set; }
        public bool IsPass { get; set; }
        public int PassLine { get; set; }

        /// <summary>
        /// 开始考试时间
        /// </summary>
        public long StartTimePoint { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public long SubmitTimePoint { get; set; }


        public List<questionDto> Questions { get; set; }



    }

    public class questionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public bool IsMulti { get; set; }
        public List<AnswerOptionEntry> AnswerOptions { get; set; }

        public string RightAnswer { get; set; }
        public string ImgUrl { get; set; }
    }


    public class SubmitExamDTO
    {
        public int ExamId { get; set; }
        public int ExamRecordId { get; set; }
        public int AdminId { get; set; }
        public Answerdetail[] AnswerDetail { get; set; }
    }

    public class Answerdetail
    {
        public int QuestionId { get; set; }
        public string[] CheckedAnswer { get; set; }
    }
}
