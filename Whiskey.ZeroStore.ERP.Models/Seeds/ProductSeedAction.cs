using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Web;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ProductSeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 2; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            //string strUrl = ConfigurationHelper.GetAppSetting("WebUrl");
             List<ProductAttribute> productAttributes = new List<ProductAttribute>()
             {
                #region 风格
                new ProductAttribute{AttributeName="风格",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="01",
                Children= new List<ProductAttribute>(){
                 new ProductAttribute{AttributeName="优雅",Description="有女人味、有品味、讲究、大气、偏保守",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_1elegant@2x.png",CodeNum="0101"},
                 //new ProductAttribute{AttributeName="自然",Description="简洁、利落、帅气、偏中性",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_2nature@2x.png",CodeNum="0102"},
                 //new ProductAttribute{AttributeName="性感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_3sexy@2x.png",CodeNum="0103"},
                 new ProductAttribute{AttributeName="干练",Description="简洁、利落、帅气、偏中性",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                 new ProductAttribute{AttributeName="知性",Description="简洁、得体、低调、中规中矩",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                 new ProductAttribute{AttributeName="文艺",Description="宽松、飘逸、森女、棉麻、民族、中式、繁复、性冷淡",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                 new ProductAttribute{AttributeName="中式",Description="旗袍、中式颜色、中式印花等中国元素",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                 new ProductAttribute{AttributeName="时尚",Description="潮流元素、较大胆、特别的设计细节、浓浓的设计感",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                 new ProductAttribute{AttributeName="酷感",Description="各种酷、前卫、张扬、不常见、独特",AttributeLevel=0,Sequence=0, OperatorId=1,IconPath="/Content/Images/ProductAttrIcon/nav_4capable@2x.png",CodeNum="0104"},
                }},
                #endregion

                #region 场合
                new ProductAttribute{AttributeName="场合",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="02",
                Children= new List<ProductAttribute>(){                 
                 new ProductAttribute(){ AttributeName="日常办公",IconPath="/Content/Images/SituationIcon/1office@2x.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0201"},
                 new ProductAttribute(){ AttributeName="商务会见",IconPath="/Content/Images/SituationIcon/1office@2x.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0202"},
                 //new ProductAttribute(){ AttributeName="私人约会",IconPath="/Content/Images/SituationIcon/2date@2x.png" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0202"},
                 new ProductAttribute(){ AttributeName="居家",IconPath="/Content/Images/SituationIcon/3home@2x.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0203"},
                 new ProductAttribute(){ AttributeName="度假旅行",IconPath="/Content/Images/SituationIcon/4travel.png" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0204"},
                 new ProductAttribute(){ AttributeName="正式宴会",IconPath="/Content/Images/SituationIcon/5banquet@2x.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0205"},
                 //new ProductAttribute(){ AttributeName="派对",IconPath="/Content/Images/SituationIcon/6party@2x.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0206"},
                 new ProductAttribute(){ AttributeName="居家运动",IconPath="/Content/Images/SituationIcon/7exercise@2x.png" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0207"},
                 new ProductAttribute(){ AttributeName="休闲娱乐",IconPath="/Content/Images/SituationIcon/8leisure.png",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0208"},
                }},
                #endregion

                #region 上装
		        new ProductAttribute{AttributeName="上装",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="03",
                   Children= new List<ProductAttribute>(){                 
                    new ProductAttribute(){ AttributeName="衬衫",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0301"},
                    new ProductAttribute(){ AttributeName="打底衫",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0302"},
                    new ProductAttribute(){ AttributeName="针织衫",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0303"},
                    new ProductAttribute(){ AttributeName="露肩装",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0304"},
                    new ProductAttribute(){ AttributeName="露背装",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0305"},
                    new ProductAttribute(){ AttributeName="低胸上衣",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0306"},
                    new ProductAttribute(){ AttributeName="西装",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0307"},
                    new ProductAttribute(){ AttributeName="外套",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0308"},
                    new ProductAttribute(){ AttributeName="卫衣",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0309"},
                    new ProductAttribute(){ AttributeName="马甲",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030a"},
                    new ProductAttribute(){ AttributeName="皮衣",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030b"},
                    new ProductAttribute(){ AttributeName="棉服",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030c"},
                    new ProductAttribute(){ AttributeName="羽绒服",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030d"},
                    new ProductAttribute(){ AttributeName="长袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030e"},
                    new ProductAttribute(){ AttributeName="中袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030f"},
                    new ProductAttribute(){ AttributeName="短袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030g"},
                    new ProductAttribute(){ AttributeName="无袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030h"},
                    new ProductAttribute(){ AttributeName="中长款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030j"},
                    new ProductAttribute(){ AttributeName="短款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="030k"},
                }},
	            #endregion

                #region 裤子
	            new ProductAttribute{AttributeName="裤子",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="04",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="铅笔裤（小脚裤）",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0401"},
                           new ProductAttribute(){ AttributeName="打底裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0402"},
                           new ProductAttribute(){ AttributeName="西裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0403"},
                           new ProductAttribute(){ AttributeName="阔腿裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0404"},
                           new ProductAttribute(){ AttributeName="靴裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0405"},
                           new ProductAttribute(){ AttributeName="休闲裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0406"},
                           new ProductAttribute(){ AttributeName="牛仔裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0407"},
                           new ProductAttribute(){ AttributeName="背带裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0408"},
                           new ProductAttribute(){ AttributeName="哈伦裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0409"},
                           new ProductAttribute(){ AttributeName="短裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="040a"},
                           new ProductAttribute(){ AttributeName="五分裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="040b"},
                           new ProductAttribute(){ AttributeName="七分裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="040c" },
                           new ProductAttribute(){ AttributeName="九分裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="040d"},
                       }},
	           #endregion

                #region 半裙
		        new ProductAttribute{AttributeName="半裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="05",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="筒裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0501"},
                           new ProductAttribute(){ AttributeName="包臀裙",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0502"},
                           new ProductAttribute(){ AttributeName="伞裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0503"},
                           new ProductAttribute(){ AttributeName="铅笔裙",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0504"},
                           new ProductAttribute(){ AttributeName="西装裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0505"},
                           new ProductAttribute(){ AttributeName="鱼尾裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0506"},
                           new ProductAttribute(){ AttributeName="A裙",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0507"},
                           new ProductAttribute(){ AttributeName="长裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0508"},
                           new ProductAttribute(){ AttributeName="中裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0509"},
                           new ProductAttribute(){ AttributeName="短裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="050a"},                                                
                       }},
	             #endregion

                #region 连衣裙
		        new ProductAttribute{AttributeName="连衣裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="06",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0601"},
                           new ProductAttribute(){ AttributeName="中裙",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0602"},
                           new ProductAttribute(){ AttributeName="短裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0603"},
                           new ProductAttribute(){ AttributeName="长袖",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0604"},
                           new ProductAttribute(){ AttributeName="中袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0605"},
                           new ProductAttribute(){ AttributeName="短袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0606"},
                           new ProductAttribute(){ AttributeName="无袖",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0607"},
                           new ProductAttribute(){ AttributeName="毛衣裙",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0608"},                                                                           
                       }},
	             #endregion

                #region 风衣
		        new ProductAttribute{AttributeName="风衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="07",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0701"},
                           new ProductAttribute(){ AttributeName="中长款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0702"},
                           new ProductAttribute(){ AttributeName="厚款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0703"},
                           new ProductAttribute(){ AttributeName="薄款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0704"},
                           new ProductAttribute(){ AttributeName="长袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0705"},
                           new ProductAttribute(){ AttributeName="中袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0706"},
                           new ProductAttribute(){ AttributeName="马甲",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0707"},                           
                       }},
	             #endregion

                #region 大衣
		        new ProductAttribute{AttributeName="大衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="08",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0801"},
                           new ProductAttribute(){ AttributeName="中长款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0802"},
                           new ProductAttribute(){ AttributeName="毛呢",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0803"},
                           new ProductAttribute(){ AttributeName="棉服",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0804"},
                           new ProductAttribute(){ AttributeName="马甲",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0805"},
                       }},
	             #endregion

                #region 羽绒服
		        new ProductAttribute{AttributeName="羽绒服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="09",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0901"},
                           new ProductAttribute(){ AttributeName="中长款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0902"},
                           new ProductAttribute(){ AttributeName="短款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0903"},
                           new ProductAttribute(){ AttributeName="马甲",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0904"},
                       }},
	             #endregion

                #region 连体裤
		        new ProductAttribute{AttributeName="连体裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0A",
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0A01"},
                           new ProductAttribute(){ AttributeName="短袖",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0A02"},
                           new ProductAttribute(){ AttributeName="无袖",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 ,CodeNum="0A03"},                           
                           new ProductAttribute(){ AttributeName="长裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0A04"},                           
                           new ProductAttribute(){ AttributeName="中裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0A05"},
                           new ProductAttribute(){ AttributeName="短裤",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1,CodeNum="0A06"},
                       }},
	             #endregion

                #region 套装
		        new ProductAttribute{AttributeName="套装",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="套装裤",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="套装裙",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                           
                       }},
	             #endregion

                #region 礼服
		        new ProductAttribute{AttributeName="礼服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="长款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="中长款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="短款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                           new ProductAttribute(){ AttributeName="低胸",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="露肩",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="露背",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="宽松",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="修身",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                       }},
	             #endregion

                #region 包
		        new ProductAttribute{AttributeName="包",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="大包",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="中包",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="小包",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                           new ProductAttribute(){ AttributeName="真皮",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="高性能皮",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="帆布",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="特色织物",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="手提包",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="单肩包",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="斜挎包",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="双肩背",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="手拿包",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                       }},
	             #endregion

                #region 鞋
		        new ProductAttribute{AttributeName="鞋",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="高跟",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="中跟",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="坡跟",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                           new ProductAttribute(){ AttributeName="平跟",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="尖头鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="凉鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="长靴",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="中靴",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="踝靴",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="系带鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="休闲鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="皮鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="帆布鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="运动鞋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                       }},
	             #endregion

                #region 围巾
		        new ProductAttribute{AttributeName="围巾",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="薄款",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="厚款",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="真丝",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                           new ProductAttribute(){ AttributeName="毛",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="羊绒",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="棉",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="长巾",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="方巾",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="围脖",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="披肩",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="大号",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="中号",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="小号",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                           
                       }},
	             #endregion

                #region 配饰
		        new ProductAttribute{AttributeName="配饰",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                          Children= new List<ProductAttribute>(){                 
                           new ProductAttribute(){ AttributeName="腰带",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="墨镜",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="耳环",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                           new ProductAttribute(){ AttributeName="项链",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="手链",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute(){ AttributeName="胸针",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                                                       
                       }},
	             #endregion

                #region 款式
	            new ProductAttribute{AttributeName="款式",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                        Children= new List<ProductAttribute>(){                 
                         new ProductAttribute(){ AttributeName="上装",IconPath="",Description="判断标准不可光腿单穿",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                           new ProductAttribute{ AttributeName="套头衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                             new ProductAttribute{ AttributeName="套头内搭毛衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="套头大毛衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="毛背心",Description="针织毛背心",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="卫衣",Description="面料偏厚的、挡风保暖的",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="打底衫",Description="特指修身款内搭",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="吊带衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="T恤",Description="各种可外穿的文化衫、有弹性、无拉链",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="套头衫",Description="薄款、无弹性，打开拉链或扣子才能穿入",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                           new ProductAttribute{ AttributeName="开衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children = new List<ProductAttribute>(){
                            new ProductAttribute{ AttributeName="衬衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="夹克",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="棒球衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="蝙蝠衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="机车小皮衣",Description="短小、修身款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="皮外套",Description="宽松、廓形款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="小西装",Description="短小、修身款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="西装外套",Description="宽松、中长款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="小马甲",Description="非夹层的、修身款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="棉马甲",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="羽绒马甲",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短外套",Description="长度在腰附近、无夹层",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="中长外套",Description="长度在臀附近、无夹层",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短棉服",Description="长度在腰附近、夹棉",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="中长棉服",Description="长度在臀附近、夹棉",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短羽绒服",Description="长度在腰附近、夹羽绒",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="中长羽绒服",Description="长度在臀附近、夹羽绒",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="空调衫",Description="薄款针织或梭织开衫",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="针织开衫",Description="针织衫",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短开衫",Description="厚款、梭织",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="中长开衫",Description="厚款",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="牛仔外套",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短风衣",Description="风衣款外套",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短斗篷",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="短皮草",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                            new ProductAttribute{ AttributeName="抹胸上衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                         }},
                         new ProductAttribute(){ AttributeName="下装",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                           new ProductAttribute{ AttributeName="裤子",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>{
                             new ProductAttribute(){AttributeName="西裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="哈伦裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="吊裆裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="铅笔裤",Description="可以露出臀部的",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="打底裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="紧身牛仔裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="男友风牛仔裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="牛仔阔腿裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="阔腿皮裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="紧身皮裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="呢料阔腿裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="薄款阔腿裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="靴裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="灯笼裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="束口裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="喇叭裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute(){AttributeName="背带裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                           new ProductAttribute{ AttributeName="半裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>{
                             new ProductAttribute{AttributeName="筒裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="A裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="伞裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="花苞裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="铅笔裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="鱼尾裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="百褶裙",Description="包括死褶和活褶的",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="西装裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="信封裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="纽扣裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="牛仔裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="燕尾裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{AttributeName="超短裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                         }},
                         new ProductAttribute(){ AttributeName="长外衣",IconPath="",Description="长度标准：可以光腿单穿",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                           new ProductAttribute{ AttributeName="连衣裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children =new List<ProductAttribute>{
                             new ProductAttribute{ AttributeName="吊带裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="背带裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="超短裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="马甲裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="衬衫裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="牛仔裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="小黑裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="小白裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="铅笔裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="鱼尾裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="A裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="百褶裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="公主裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="直身裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="毛衣裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="旗袍裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长袍裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="小礼服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="大礼服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="连体裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                           new ProductAttribute{ AttributeName="套装",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                             new ProductAttribute{ AttributeName="西服套装裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="西服套装裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="厚款套装裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="厚款套装裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="薄款套装裤",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="薄款套装裙",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                           new ProductAttribute{ AttributeName="长外衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                             new ProductAttribute{ AttributeName="英伦长风衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="拉风长衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长马甲",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长开衫",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="呢大衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="皮大衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="针织大衣",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长款棉服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长款羽绒服",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                             new ProductAttribute{ AttributeName="长款皮草",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           }},
                         }},
                         new ProductAttribute(){ AttributeName="鞋",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                         }},
                         new ProductAttribute(){ AttributeName="包",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                         }},
                         new ProductAttribute(){ AttributeName="围巾",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                         }},
                         new ProductAttribute(){ AttributeName="配饰",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                           new ProductAttribute{ AttributeName="太阳镜",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute{ AttributeName="耳环",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute{ AttributeName="项链",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute{ AttributeName="戒指",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute{ AttributeName="手链",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                           new ProductAttribute{ AttributeName="腰带",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         }},
                     }},
	            #endregion

                #region 颜色
	            new ProductAttribute{AttributeName="颜色",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                      Children= new List<ProductAttribute>(){                                         
                       new ProductAttribute(){ AttributeName="黑色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="白色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="灰色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="粉色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="红色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="橙色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="黄色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="绿色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="蓝色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="青色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="紫色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="卡其色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="棕色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="杂色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="金色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="银色",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="印花",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="格子",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="波点",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                       new ProductAttribute(){ AttributeName="条纹",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children= new List<ProductAttribute>(){
                       }},
                   }},
	            #endregion

                #region 实测尺寸表
                   new ProductAttribute{AttributeName="实测尺寸表",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                        },
	            #endregion

                #region 面料性能表
	            new ProductAttribute{AttributeName="面料性能表",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                      new ProductAttribute{AttributeName="厚度",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                         new ProductAttribute{AttributeName="厚",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="适中",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="薄",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                      }}, 
                      new ProductAttribute{AttributeName="弹性",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                         new ProductAttribute{AttributeName="无弹",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="微弹",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="较高",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="高弹",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                      }}, 
                      new ProductAttribute{AttributeName="手感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                         new ProductAttribute{AttributeName="挺括",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="厚实",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="致密",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="轻薄",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="柔软",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="细滑",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="温暖感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="凉爽感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="毛绒感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="飘逸感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="肌理感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="光泽感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="网眼",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="钩花",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="金丝",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="银丝",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="亮片",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="立体装饰",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="流苏",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="花边",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="滚边",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="拼接",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         
                      }}, 
                      new ProductAttribute{AttributeName="抗皱",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,Children=new List<ProductAttribute>(){
                         new ProductAttribute{AttributeName="强",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="适中",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                         new ProductAttribute{AttributeName="弱",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                      }}, 
                  }}, 
	            #endregion

                #region 体型
	            new ProductAttribute{AttributeName="体型",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="X型",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="I型",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="A型",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="V型",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="H型",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="O型",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                                                       
                    }},
	            #endregion

                #region 特型
	            new ProductAttribute{AttributeName="特型",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="脖子短",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="上臂粗",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="肩宽",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="胸大",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="腰粗",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="腹部突出",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="宽臀",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="窄臀",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="平臀",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="大腿粗",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="小腿粗",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="腿不直",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                    }},
	            #endregion

                #region 季节
	            new ProductAttribute{AttributeName="季节",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="夏季",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="冬季",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="春秋",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="春夏秋",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="春秋冬",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="四季",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                         
                    }},	 
	            #endregion

                #region 效果
	            new ProductAttribute{AttributeName="效果",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="显高",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="提亮肤色",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="显瘦",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="丰满",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="减龄",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="成熟",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="严谨",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="率性",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="权威",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="亲和",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="华丽",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="简约大气",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="性感",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="中性",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="活力",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="安静",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="时尚",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="传统",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="飘逸",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="踏实",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="考究",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="清新",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="职业",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="知性",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="艺术",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="独特",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="酷",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="优雅",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="异域",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="自然",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="文艺",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="街头",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="个性",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                    }},	 
	            #endregion

                #region 存在感
	            new ProductAttribute{AttributeName="存在感",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="张扬",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="低调",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="强",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="中",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="弱",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},                        
                    }},	 
	            #endregion

                #region 穿着方式
	            new ProductAttribute{AttributeName="穿着方式",Description="",AttributeLevel=0,Sequence=0, OperatorId=1,
                       Children= new List<ProductAttribute>(){                 
                        new ProductAttribute(){ AttributeName="套头",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="开衫、无扣、无拉链",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="前拉链",IconPath="",Description="",AttributeLevel=0,Sequence=0, OperatorId=1 },                           
                        new ProductAttribute(){ AttributeName="后拉链",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="前系扣",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="后系扣",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="侧拉链",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="侧系扣",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                        new ProductAttribute(){ AttributeName="松紧腰",IconPath="" ,Description="",AttributeLevel=0,Sequence=0, OperatorId=1},
                    }},
	            #endregion

            };
            List<string> listFrist = new List<string>();
            List<string> listSecond = new List<string>();
            List<string> listThird = new List<string>();
            //ASCII  0-9 48-57
            //       A-Z 65-90
            int numStart = 48;
            int numEnd = 57;
            int letterStart = 65;
            int letterEnd = 90;
            string codeNum = string.Empty;
            StringBuilder sbSecondNum = new StringBuilder();            
            StringBuilder sbFrist = new StringBuilder();
            StringBuilder sbSecond = new StringBuilder();
            StringBuilder sbThird = new StringBuilder();
            Random random = new Random();
            int start = numStart;
            int end = numStart;
            this.Get(productAttributes,codeNum);
            foreach (var item in productAttributes)
            {
                codeNum = item.CodeNum;
                this.Get(item.Children.ToList(), codeNum);
                foreach (var temp in item.Children)
                {
                    codeNum = temp.CodeNum;
                    this.Get(temp.Children.ToList(),codeNum);
                    foreach (var pro in temp.Children)
                    {
                        codeNum = pro.CodeNum;
                        this.Get(pro.Children.ToList(), codeNum);
                    }
                }                                                         
            }
            context.Set<ProductAttribute>().AddOrUpdate(productAttributes.ToArray());
            context.SaveChanges();


            #region 人群分类

            ProductCrowd modPC = new ProductCrowd() { CrowdCode = "NZ", CrowdName = "女装" };
            context.Set<ProductCrowd>().AddOrUpdate(modPC);
            context.SaveChanges();

            #endregion

        }

        private void Get(List<ProductAttribute> productAttributes, string codeNum)
        {
            List<string> listFrist = new List<string>();                    
            //ASCII  0-9 48-57
            //       A-Z 65-90
            int numStart = 48;
            int numEnd = 57;
            int letterStart = 65;
            int letterEnd = 90;             
            StringBuilder sbSecondNum = new StringBuilder();
            StringBuilder sbFrist = new StringBuilder();             
            Random random = new Random();
            int start = numStart;
            int end = numStart;
            foreach (var item in productAttributes)
            {
                while (true)
                {
                    sbFrist.Clear();
                    if (end == numEnd)
                    {
                        end = letterStart;
                    }
                    else if (end == letterEnd)
                    {
                        end = numStart;
                        if (start < numEnd)
                        {
                            start += 1;
                        }
                        else
                        {
                            start = letterStart;
                        }
                    }
                    else
                    {
                        end += 1;
                    }
                    byte[] array = { (byte)start, (byte)end };
                    sbFrist.Append(Encoding.ASCII.GetString(array));
                    if (listFrist.Contains(sbFrist.ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        listFrist.Add(codeNum);
                        item.CodeNum =codeNum+ sbFrist.ToString();
                        break;
                    }
                }
            }            
        }

     }
}


