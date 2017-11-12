using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class PageDto
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public int pageCount
        {
            get
            {
                return (int)Math.Ceiling(totalCount * 1.0 / pageSize);
            }
        }
    }
}