using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models
{
    public class ModuPermission
    {
        public ModuPermission()
        {
            Child = new List<ModuPermission>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ModuPermission> Child { get; set; }
    }
}