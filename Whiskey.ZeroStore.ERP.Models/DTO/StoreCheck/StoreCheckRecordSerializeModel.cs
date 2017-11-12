using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 店铺巡查记录序列化结构
    /// </summary>
    public class StoreCheckRecordSerializeModel
    {
        public StoreCheckRecordSerializeModel()
        {
            CheckDetails = new List<CheckDetail>();
        }
        public int Id { get; set; }
        public string CheckName { get; set; }
        public string Desc { get; set; }
        public int ItemsCount { get; set; }
        public int PunishScore { get; set; }
        public int Standard { get; set; }
        public int GetScore { get; set; }
        public List<CheckDetail> CheckDetails { get; set; }

    }
}
