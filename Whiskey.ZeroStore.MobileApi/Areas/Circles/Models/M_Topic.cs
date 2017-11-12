using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Circles.Models
{
    public class M_Topic
    {
        public string MemberId { get; set; }
        public string CircleId { get; set; }
        public string TopicName { get; set; }
        public string Content { get; set; }
    }
}