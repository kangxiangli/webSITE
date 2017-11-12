using System;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Products
{
    [Serializable]
    public class GetListByStoreRes
    {
        public int StoreId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public int ColorId { get; set; }
        public string BigProdNum { get; set; }
        public string CategoryName { get; set; }
        public string SeasonName { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public float Price { get; set; }
        public string ImagePath { get; set; }
        public string ColorIconPath { get; set; }

        public string ImageOrgPath { get; set; }
        public string HtmlPath { get; set; }
    }
}