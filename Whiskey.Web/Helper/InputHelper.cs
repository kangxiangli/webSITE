using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    public static class InputHelper
    {
        public static string SafeInput(string html)
        {
            if (html != null)
            {
                html = html.Trim().Replace("%", "").Replace("'", "").Replace("\"", "").Replace(";", "").Replace("<", "").Replace("%", "").Replace(">", "").Replace("exec", "").Replace("master", "").Replace("truncate", "").Replace("declare", "").Replace("xp_", "no");
            }
            return html;
        }

    }
}
