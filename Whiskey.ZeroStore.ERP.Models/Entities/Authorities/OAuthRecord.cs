using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class OAuthRecord : EntityBase<int>
    {
        /// <summary>
        /// 第三方授权标识
        /// </summary>
        public virtual string OpenId { get; set; }
        /// <summary>
        /// 授权类型
        /// </summary>
        public virtual ThirdLoginFlag ThirdLoginFlag { get; set; }
    }
}
