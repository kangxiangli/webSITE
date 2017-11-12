
using System;
using System.ComponentModel;
using System.Reflection;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    public enum InventoryStatus
    {
        /*
         * 2016-7
         * yxk
           对于库存状态作如下约定：
         *  状态码 在
         * 0     :表示正常状态，可以采购，销售
         * [1,10] :都表示采购阶段，未使用的状态码应该予以保留
         *    1： 采购开始的状态，没有实际意义，仅表示该区间的开始，该状态在实际中不应该被使用
         *    2： 采购锁定
         *    10： 采购阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         *    
         *
         * [11,20]:都表示配货阶段，未使用的状态码应予以保留
         *    11：配货阶段开始的状态，没有实际意义，仅表示区间的开始，该状态在实际中不应该被使用
         *    12：配货锁定
         *    13：配货完成，待接收
         *    20：配货阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         *    
         * [21,30]:都表示零售阶段，未使用的状态码应该予以保留
         *    21：零售阶段开始的状态，没有实际意义，仅表示区间的开始，该状态在实际中不应该被使用
         *    22：零售锁定，默认15分钟超时
         *    23：已加入订单（对于零售加入订单即意味出库）
         *    30：零售阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         *    
         * [31,40]:都表示退货阶段，未使用的状态码应予以保留
         *    31：退货阶段开始的状态，没有实际意义，仅表示区间的开始，该状态在实际中不应该被使用
         *    32：退货
         *    40：退货阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         *    
         * [41,50]:都表示欠损，未使用的状态码应予以保留
         *    41：欠损阶段开始的状态，没有实际意义，仅表示区间的开始，该状态在实际中不应该被使用
         *    42：欠损
         *    50：欠损阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         *    
         * [51,60]:都表示其他原因锁定，此阶段的商品不能采购，零售
         *    51：阶段开始的状态，没有实际意义，仅表示区间的开始，该状态在实际中不应该被使用
         *    60：阶段结束的状态，没有实际意义，仅表示该区间的结束，该状态在实际中不应该被使用
         */


        /// <summary>
        /// 正常状态可卖，可采购，默认状态，0
        /// </summary>
        [Description("待采购，待销售")]
        Default = 0,

        #region 采购阶段
        /// <summary>
        /// 该状态仅表示采购阶段开始，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("采购阶段开始")]
        PurchasStart = 1,
        /// <summary>
        /// 采购锁定，但是未出库，2
        /// </summary>
        [Description("采购锁定")]
        Purchased = 2,
        /// <summary>
        /// 该状态仅表示采购阶段结束，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("采购阶段结束")]
        PurchasEnd = 10,
        #endregion

        #region 配货阶段
        /// <summary>
        /// 仅表示配货阶段开始状态，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("配货阶段开始")]
        DeliveryStart = 11,
        /// <summary>
        /// 配货锁定
        /// </summary>
        [Description("配货锁定")]
        DeliveryLock = 12,
        /// <summary>
        /// 配货待接收
        /// </summary>
        [Description("配货待接收")]
        UnAccept = 13,
        /// <summary>
        /// 仅表示配货阶段结束状态，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("配货阶段结束")]
        DeliveryEnd = 20,
        #endregion

        #region 销售阶段
        /// <summary>
        /// 仅表示销售阶段开始，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("销售阶段开始")]
        SaleStart = 21,
        /// <summary>
        /// 销售页面锁定,已启用,改为在缓存中锁定
        /// </summary>
        [Obsolete("已启用,改为在缓存中锁定", true)]
        [Description("销售锁定")]
        SaleLock = 22,
        /// <summary>
        /// 已加入订单，23，原来对应的是5，对于商品零售，加入订单即意味着出库
        /// </summary>
        [Description("加入销售订单")]
        JoinOrder = 23,
        /// <summary>
        /// 仅表示销售阶段结束，没有实际意义，在实际中不应该被使用
        /// </summary>
        [Description("销售阶段结束")]
        SaleEnd = 30,


        #endregion


        



    }

    public static class InventoryStatusExten
    {
        public static string GetDescriptionText(this InventoryStatus enumName)
        {
            string str = enumName.ToString();
            FieldInfo field = enumName.GetType().GetField(str);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0) return str;
            DescriptionAttribute da = (DescriptionAttribute)objs[0];
            return da.Description;
        }
    }

}
