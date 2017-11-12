using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Whiskey.Core.Data.Entity.Migrations;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MaterualsSeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 10; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            //商品素材实体
            List<Gallery> Gallerys = new List<Gallery>()
            {
                new Gallery {  PictureName="裙子",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC1@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC1@2x.png", Sequence=0},
                new Gallery {  PictureName="围脖",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC2@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC2@2x.png", Sequence=0},
                new Gallery {  PictureName="外套",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC3@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC3@2x.png", Sequence=0},
                new Gallery {  PictureName="高跟鞋",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC4@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC4@2x.png", Sequence=0},
                new Gallery {  PictureName="模型",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC5@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC5@2x.png",Sequence=0},
                new Gallery {  PictureName="平跟鞋",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC6@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC6@2x.png", Sequence=0},
                new Gallery {  PictureName="手提包",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC7@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC7@2x.png",Sequence=0},
                new Gallery {  PictureName="领带",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC8@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC8@2x.png", Sequence=0},
                new Gallery {  PictureName="泳装",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC9@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC9@2x.png", Sequence=0},
                new Gallery {  PictureName="裤子",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC10@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC10@2x.png",Sequence=0},
                new Gallery {  PictureName="眼镜",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC11@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC11@2x.png", Sequence=0},
                new Gallery {  PictureName="雨伞",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC12@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC12@2x.png", Sequence=0},
                new Gallery {  PictureName="长筒靴",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC13@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC13@2x.png", Sequence=0},
                new Gallery {  PictureName="文胸",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC14@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC14@2x.png",Sequence=0},
                new Gallery {  PictureName="戒指",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC15@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC15@2x.png", Sequence=0},
                new Gallery {  PictureName="包",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC16@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC16@2x.png", Sequence=0},
                new Gallery {  PictureName="耳坠",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC17@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC17@2x.png", Sequence=0},
                new Gallery {  PictureName="缝纫机",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC18@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC18@2x.png",Sequence=0},
                new Gallery {  PictureName="剪刀",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC19@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Product/SC19@2x.png",Sequence=0},
                new Gallery {  PictureName="胸针",GalleryType=GalleryFlag.Product,ThumbnailPath="/Content/UploadFiles/Galleries/Product/SC20@2x.png",OriginalPath="/Content/UploadFiles/Galleries/Product/SC20@2x.png", Sequence=0},
            };
            List<Gallery> listGallery = new List<Gallery>() {
                new Gallery {  PictureName="蔚蓝",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/1.png",OriginalPath="/Content/UploadFiles/Galleries/Background/1.png", Sequence=0},
                new Gallery {  PictureName="黑色",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/2.png", OriginalPath="/Content/UploadFiles/Galleries/Background/2.png", Sequence=0},
                new Gallery {  PictureName="科技",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/3.png",OriginalPath="/Content/UploadFiles/Galleries/Background/3.png", Sequence=0},
                new Gallery {  PictureName="天空蓝",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/4.png",OriginalPath="/Content/UploadFiles/Galleries/Background/4.png",  Sequence=0},
                new Gallery {  PictureName="钻石红",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/5.png",OriginalPath="/Content/UploadFiles/Galleries/Background/5.png", Sequence=0},
                new Gallery {  PictureName="荧光黄",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/6.png",OriginalPath="/Content/UploadFiles/Galleries/Background/6.png", Sequence=0},
                new Gallery {  PictureName="纹理",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/7.png", OriginalPath="/Content/UploadFiles/Galleries/Background/7.png",Sequence=0},
                new Gallery {  PictureName="层级",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/8.png", OriginalPath="/Content/UploadFiles/Galleries/Background/8.png", Sequence=0},
                new Gallery {  PictureName="英伦",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/9.png", OriginalPath="/Content/UploadFiles/Galleries/Background/9.png",Sequence=0},
                new Gallery {  PictureName="火车",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/10.png",OriginalPath="/Content/UploadFiles/Galleries/Background/10.png", Sequence=0},
                new Gallery {  PictureName="白色",GalleryType=0,ThumbnailPath="/Content/UploadFiles/Galleries/Background/bg@2x.png", OriginalPath="/Content/UploadFiles/Galleries/Background/bg@2x.png",Sequence=0},
            };
            Gallerys.AddRange(listGallery);

            context.Set<Gallery>().AddOrUpdate(Gallerys.ToArray());
            context.SaveChanges();

        }
    }
}