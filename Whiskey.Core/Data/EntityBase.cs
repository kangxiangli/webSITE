
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Whiskey.Core.Data
{
    /// <summary>
    /// 可持久化到数据库的数据模型基类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class EntityBase<TKey>
    {
        protected EntityBase()
        {
            Sequence = 0;
            IsEnabled = true;
            IsDeleted = false;
            CreatedTime = DateTime.Now;
            UpdatedTime = DateTime.Now;
        }

        #region 属性

        /// <summary>
        /// 获取或设置 实体唯一标识，主键
        /// </summary>
        [Display(Name = "实体标识")]
        [Key]
        public TKey Id { get; set; }

        /// <summary>
        /// 获取或设置 是否删除，逻辑上的删除，非物理删除
        /// </summary>
        [Display(Name = "是否删除")]
        [Index]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 获取或设置 是否启用或禁用
        /// </summary>
        [Display(Name = "是否启用")]
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// 排序序号
        /// </summary>
        [Display(Name = "排序序号")]
        [Index]
        public int Sequence { get; set; }

        /// <summary>
        /// 获取或设置 更新时间
        /// </summary>
        [Display(Name = "更新时间")]
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 获取或设置 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [Index]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 操作员ID
        /// </summary>
        [Display(Name = "操作人")]
        public int? OperatorId { get; set; }

        /// <summary>
        /// 获取或设置 版本控制标识，用于处理并发
        /// </summary>
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Timestamp { get; set; }
        //public string Others { get; set; }

        #endregion

        #region 方法

        /// <summary>
        /// 判断两个实体是否是同一数据记录的实体
        /// </summary>
        /// <param name="obj">要比较的实体信息</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            EntityBase<TKey> entity = obj as EntityBase<TKey>;
            if (entity == null)
            {
                return false;
            }
            return Id.Equals(entity.Id) && CreatedTime.Equals(entity.CreatedTime);
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns>
        /// 当前 <see cref="T:System.Object"/> 的哈希代码。
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ CreatedTime.GetHashCode();
        }

        #endregion
    }
}