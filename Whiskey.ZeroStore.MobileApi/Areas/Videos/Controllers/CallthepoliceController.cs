using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Data;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Whiskey.ZeroStore.MobileApi.Areas.Videos.Controllers
{
    public class CallthepoliceController : Controller
    {

        public readonly ICallthepoliceContract _callthepoliceContract;

        public CallthepoliceController(ICallthepoliceContract callthepoliceContract)
        {
            _callthepoliceContract = callthepoliceContract;
        }

        public JsonResult WriteLog()
        {
            var oper = new OperationResult(OperationResultType.Success);
            try
            {
                System.IO.Stream s = Request.InputStream;
                int count = 0;
                byte[] buffer = new byte[1024];
                StringBuilder builder = new StringBuilder();
                while ((count = s.Read(buffer, 0, 1024)) > 0)
                {
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
                }
                s.Flush();
                s.Close();
                s.Dispose();

                string paramsStr = builder.ToString();

                var list = new List<Callthepolice>();
                if (!string.IsNullOrEmpty(paramsStr))
                {
                    Callthepolice cp = new Callthepolice();
                    JObject json = (JObject)JsonConvert.DeserializeObject(paramsStr);
                    dynamic jsonT = JToken.Parse(json["params"].ToString()) as dynamic;
                    cp.alarmId = jsonT.id.ToString();
                    cp.cid = Convert.ToInt32(jsonT.cid.ToString());
                    cp.cname = jsonT.cname;
                    cp.did = jsonT.did;
                    cp.time = jsonT.time;
                    cp.type = Convert.ToInt32(jsonT.type.ToString());
                    cp.CreatedTime = DateTime.Now;
                    list.Add(cp);
                    oper = _callthepoliceContract.Insert(list.ToArray());
                }
            }
            catch (Exception e)
            {

            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
    }
}