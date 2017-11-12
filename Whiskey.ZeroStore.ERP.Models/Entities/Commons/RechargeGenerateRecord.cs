



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{


    public class RechargeGenerateRecord : EntityBase<int>
    {
        public RechargeGenerateRecord()
        {
            MemberDeposits = new List<MemberDeposit>();
        }
        public virtual Administrator Operator { get; set; }

        [DisplayName("��ֵ����")]
        public int RechargeCount { get; set; }

        [DisplayName("��ֵ�")]
        public int MemberActivityId { get; set; }

        [ForeignKey("MemberActivityId")]
        public  MemberActivity MemberActivity { get; set; }

        [DisplayName("��ֵ��Ա")]
        public virtual ICollection<MemberDeposit> MemberDeposits { get; set; }

    }

    public class RechargeGenerateRecordConfig : EntityConfigurationBase<RechargeGenerateRecord, int>
    {
        public RechargeGenerateRecordConfig()
        {
            ToTable("RechargeGenerateRecord");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(r => r.MemberDeposits)
                .WithMany(m => m.RechargeGenerateRecords)
                .Map(config => config.MapLeftKey("RechargeGenerateRecordId")
                .MapRightKey("MemberDepositId")
                .ToTable("RechargeGenerateRecord_MemberDeposit_Relation"));
        }
    }




}


