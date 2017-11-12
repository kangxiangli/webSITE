using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models
{
    public class RightTree
    {
        public RightTree()
        {
            children = new List<RightTree>();
        }
        public string id { get; set; }
        public String url { get; set; }// tree组件一般用于菜单，url为菜单对应的地址
        public String text { get; set; }// 显示文字
        public bool _checked { get; set; }// 是否选中
        public bool _isShow { get; set; }//是否显示
        public int? _gtype { get; set; }//逻辑上的分组
        public string msg { get; set; }
        public List<RightTree> children { get; set; }// 子tree
    }
    public class ResJson
    {
        public bool success { get; set; }
        public String msg { get; set; }// 消息
        public String type { get; set; }// 类型
        public Object obj { get; set; }// 对象
    }
}