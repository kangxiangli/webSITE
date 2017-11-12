using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class CheckupItemDto : IAddDto, IEditDto<int>,ICloneable
    {
        [Display(Name = "盘点标识符")]
        [StringLength(20)]
        public virtual string CheckGuid { get; set; }

        [Display(Name = "盘点详情")]
        public virtual int? CheckerItemId { get; set; }

        /// <summary>
        /// 枚举值，参考OpertaionFlag  缺货删除；余货插入
        /// </summary>
        [DisplayName("校验类型")]
        [Description("枚举值，参考OpertaionFlag")]
        public virtual int CheckupType { get; set; }

        [DisplayName("标识Id")]
        public int Id { get; set; }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <returns></returns>
        public CheckupItemDto DeepCopy()
        {

            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream) as CheckupItemDto;
            }
        }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public CheckupItemDto ShallowClone()
        {
            return Clone() as CheckupItemDto;
        }
    }
}
