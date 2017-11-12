using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Models
{
    [Serializable]
    public class Product_Model
    {
        public Product_Model()
        {
            Id = 0;
            UUID = "";
            Thumbnail = "";
            ProductNumber = "";
            TagPrice = 0;
            WholesalePrice = 0;
            Season = "";
            Size = "";
            Color = "";
            Amount = 1;
            ValidCoun = 0;
            UpdateTime = DateTime.Now;
        }
        public int Id { get; set; }

        public string UUID { get; set; }

        public string Thumbnail { get; set; }

        public string ProductNumber { get; set; }
        public string ProductBarcode { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }

        public string Brand { get; set; }
        public float TagPrice { get; set; }//吊牌价

        public float WholesalePrice { get; set; }//进货价
       
        public string Season { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public int Amount { get; set; } //库存数
        public int Quantity { get; set; }
        public int ValidCoun { get; set; }
        public bool IsValided { get; set; }
        public string Other { get; set; }

        public DateTime UpdateTime { get; set; }
        public string Notes { get; set; }

        public int ProductId { get; set; }

        public float PurchasePrice { get; set; }

        /// <summary>
        /// 采购数量
        /// </summary>
        public int PurchaseQuantity { get; set; }
    }


}