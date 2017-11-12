using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PropertySeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 3; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            #region 品牌 Brand

            //Brand实体
            List<Brand> brands = new List<Brand>()
            {
                new Brand { BrandName="自营品牌", BrandCode="2", OperatorId=1, ParentId=null, Description="",DefaultDiscount=1, Children=new Brand[]{
                    new Brand(){BrandName="Lirica",BrandCode="NC",OperatorId=1,DefaultDiscount=1},
                    new Brand(){BrandName="Esme.Sh",BrandCode="CL",OperatorId=1,DefaultDiscount=1},
                    new Brand(){BrandName="Fanny.C",BrandCode="DA",OperatorId=1,DefaultDiscount=1},
                    new Brand(){BrandName="VVHX",BrandCode="DS",OperatorId=1,DefaultDiscount=1},
                }},
                new Brand { BrandName="外部品牌", BrandCode="2D", OperatorId=1, ParentId=null, Description="",DefaultDiscount=1, Children=new Brand[]{
                    new Brand(){BrandName="Snowimage",BrandCode="DQ",OperatorId=1,DefaultDiscount=1},
                }},
                new Brand { BrandName="联营品牌", BrandCode="2X", OperatorId=1, ParentId=null, Description="",DefaultDiscount=1, Children=new Brand[]{
                    new Brand(){BrandName="银生",BrandCode="LY",OperatorId=1,DefaultDiscount=1},
                    new Brand(){BrandName="HUTTIONN",BrandCode="HU",OperatorId=1,DefaultDiscount=1},
                    new Brand(){BrandName="LOTUS.CHEUNG",BrandCode="LO",OperatorId=1,DefaultDiscount=1},
                }}
            };
            context.Set<Brand>().AddOrUpdate(brands.ToArray());
            context.SaveChanges();

            #endregion

            //Category实体
            List<Category> categorys = new List<Category>()
            {
                new Category(){OperatorId=1,CategoryName="上装",CategoryCode="A8",Children=new Category[]{
                    new Category(){OperatorId=1,CategoryName="套头衫",CategoryCode="A2" ,IconPath= "/Content/Images/CategoryIcon/tab_1slip-onshirt_pre@2x.png",Sizes=new List<Size>() { new Size() { SizeName = "M", SizeCode = "M" } } },
                }},
            };
            context.Set<Category>().AddOrUpdate(categorys.ToArray());

            #region 颜色 Color

            List<Color> listColor = new List<Color>()
            {
                new Color() {ColorName="黑色",IconPath= "/Content/Images/ColorIcon/nav_1black@2x.png",ColorCode="D2",OperatorId=1,Description=""},
                new Color() {ColorName="白色",IconPath= "/Content/Images/ColorIcon/nav_2white@2x.png",ColorCode="IS",OperatorId=1,Description=""},
                new Color() {ColorName="灰色",IconPath= "/Content/Images/ColorIcon/nav_3grey@2x.png",ColorCode="A2",OperatorId=1,Description=""},
                new Color() {ColorName="粉色",IconPath= "/Content/Images/ColorIcon/nav_4pink@2x.png",ColorCode="Y2",OperatorId=1,Description=""},
                new Color() {ColorName="红色",IconPath= "/Content/Images/ColorIcon/nav_5red@2x.png",ColorCode="C2",OperatorId=1,Description=""},
                new Color() {ColorName="橙色",IconPath= "/Content/Images/ColorIcon/nav_6orange@2x.png",ColorCode="NN",OperatorId=1,Description=""},
                new Color() {ColorName="黄色",IconPath= "/Content/Images/ColorIcon/nav_7yellow@2x.png",ColorCode="HH",OperatorId=1,Description=""},
                new Color() {ColorName="绿色",IconPath= "/Content/Images/ColorIcon/nav_8green@2x.png",ColorCode="FF",OperatorId=1,Description=""},
                new Color() {ColorName="蓝色",IconPath= "/Content/Images/ColorIcon/nav_9blue@2x.png",ColorCode="F3",OperatorId=1,Description=""},
                new Color() {ColorName="青色",IconPath= "/Content/Images/ColorIcon/nav_10cyan@2x.png",ColorCode="N7",OperatorId=1,Description=""},
                new Color() {ColorName="紫色",IconPath= "/Content/Images/ColorIcon/nav_11purple@2x.png",ColorCode="N3",OperatorId=1,Description=""},
                new Color() {ColorName="卡其色",IconPath= "/Content/Images/ColorIcon/nav_12khaki@2x.png",ColorCode="V4",OperatorId=1,Description=""},
                new Color() {ColorName="棕色",IconPath= "/Content/Images/ColorIcon/nav_13brown@2x.png",ColorCode="H4",OperatorId=1,Description=""},
                new Color() {ColorName="杂色",IconPath= "/Content/Images/ColorIcon/nav_14mix@2x.png",ColorCode="8J",OperatorId=1,Description=""},
                new Color() {ColorName="金色",IconPath= "/Content/Images/ColorIcon/nav_15golden@2x.png",ColorCode="7H",OperatorId=1,Description=""},
                new Color() {ColorName="银色",IconPath= "/Content/Images/ColorIcon/nav_16silver@2x.png",ColorCode="S3",OperatorId=1,Description=""},
                new Color() {ColorName="印花",IconPath= "/Content/Images/ColorIcon/nav_17stamp@2x.png",ColorCode="56",OperatorId=1,Description=""},
                new Color() {ColorName="格子",IconPath= "/Content/Images/ColorIcon/nav_18lattice@2x.png",ColorCode="34",OperatorId=1,Description=""},
                new Color() {ColorName="波点",IconPath= "/Content/Images/ColorIcon/nav_19wave@2x.png",ColorCode="P1",OperatorId=1,Description=""},
                new Color() {ColorName="条纹",IconPath= "/Content/Images/ColorIcon/nav_20fringe@2x.png",ColorCode="I3",OperatorId=1,Description=""},
                new Color() {ColorName="其他",IconPath= "",ColorCode="L3",OperatorId=1,Description=""}
            };

            context.Set<Color>().AddOrUpdate(listColor.ToArray());

            #endregion

            #region 季节 Season

            List<Season> seasons = new List<Season>()
            {
                new Season(){SeasonName="夏季",SeasonCode="2",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/2sumer@2x.png"},
                new Season(){SeasonName="春秋",SeasonCode="3",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/3autumn@2x.png"},
                new Season(){SeasonName="冬季",SeasonCode="4",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/4winter@2x.png"},
                new Season(){SeasonName="春夏秋",SeasonCode="5",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/5sp_su@2x.png"},
                new Season(){SeasonName="春秋东",SeasonCode="6",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/6sp_au@2x.png"},
                new Season(){SeasonName="四季",SeasonCode="8",OperatorId=1,IconPath= "/Content/Images/SeasonIcon/8annual@2x.png"},
            };
            context.Set<Season>().AddOrUpdate(seasons.ToArray());

            #endregion

            context.SaveChanges();

        }
    }

    public class color
    {

        public color(string name, float min, float max)
        {
            Name = name;
            Min = min;
            Max = max;
        }
        public string Name { get; set; }

        public float Min { get; set; }

        public float Max { get; set; }

    }
}
