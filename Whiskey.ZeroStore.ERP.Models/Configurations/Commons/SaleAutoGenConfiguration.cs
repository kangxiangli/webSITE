
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class SaleAutoGenConfiguration : EntityConfigurationBase<SaleAutoGen, int>
    {
        public SaleAutoGenConfiguration()
        {
            ToTable("C_SaleAutoGen");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Products).WithMany(w => w.SaleAutoGens).Map(m =>
            {
                m.ToTable("C_SaleAutoGen_Product_Relation");
                m.MapLeftKey("SaleAutoGenId");
                m.MapRightKey("ProductId");
            });
            HasMany(m => m.ReceiveStorages).WithMany(w => w.SaleAutoGens).Map(m =>
            {
                m.ToTable("C_SaleAutoGen_Storage_Relation");
                m.MapLeftKey("SaleAutoGenId");
                m.MapRightKey("StorageId");
            });
        }
    }
    public class SellerMemberConfiguration : EntityConfigurationBase<SellerMember, int>
    {
        public SellerMemberConfiguration()
        {
            ToTable("C_SellerMember");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Members).WithMany(w => w.SellerMembers).Map(m =>
            {
                m.ToTable("C_SellerMember_Member_Relation");
                m.MapLeftKey("SellerMemberId");
                m.MapRightKey("MemberId");
            });
            HasMany(m => m.Sellers).WithMany(w => w.SellerMembers).Map(m =>
            {
                m.ToTable("C_SellerMember_Administrator_Relation");
                m.MapLeftKey("SellerMemberId");
                m.MapRightKey("AdministratorId");
            });
        }
    }
}

