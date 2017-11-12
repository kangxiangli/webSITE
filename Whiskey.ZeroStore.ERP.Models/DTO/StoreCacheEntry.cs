using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class StoreCacheEntry
    {

        public int Id { get; set; }

        /// <summary>
        /// 店铺余额
        /// </summary>
        public float Balance { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public String StoreName { get; set; }

        /// <summary>
        /// 店铺头像
        /// </summary>
        public String StorePhoto { get; set; }


        /// <summary>
        /// 官方总店
        /// </summary>
        public Boolean IsMainStore { get; set; }

        /// <summary>
        /// 店铺简介
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// 店铺信誉
        /// </summary>
        public Int32 StoreCredit { get; set; }


        /// <summary>
        /// 联系人
        /// </summary>
        public String ContactPerson { get; set; }


        /// <summary>
        /// 店铺电话
        /// </summary>
        public String Telephone { get; set; }

        /// <summary>
        /// 所在省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 所在城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public String ZipCode { get; set; }

        /// <summary>
        /// 店铺地址
        /// </summary>
        public String Address { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public String Notes { get; set; }

        /// <summary>
        /// 所属部门
        /// </summary>
        public int? DepartmentId { get; set; }


        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 店铺类型
        /// </summary>
        public int StoreTypeId { get; set; }
        /// <summary>
        /// 店铺类型名称
        /// </summary>
        public string StoreTypeName { get; set; }
        /// <summary>
        /// 店铺类型名称
        /// </summary>
        public DateTime StoreTypeCreateTime { get; set; }

        /// <summary>
        /// 归属店铺
        /// </summary>
        public virtual bool IsAttached { get; set; }

        /// <summary>
        /// 店铺折扣
        /// </summary>
        public virtual float StoreDiscount { get; set; }

        /// <summary>
        /// 是否已闭店
        /// </summary>
        public bool IsClosed { get; set; }
        /// <summary>
        /// 店铺类型名称
        /// </summary>
        public DateTime CreateTime { get; set; }



    }
}
