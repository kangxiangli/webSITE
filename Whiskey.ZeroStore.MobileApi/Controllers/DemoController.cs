using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    
    public class DemoController : ApiController
    {
        public IMemberContract _memberContract { get; set; }
        // GET: api/Demo
        public IEnumerable<string> Get()
        {
            string name= _memberContract.Members.Where(x => x.IsEnabled == true).FirstOrDefault().MemberName;
            return new string[] { "value1", name };
        }

        // GET: api/Demo/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Demo
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Demo/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Demo/5
        public void Delete(int id)
        {
        }
    }
}
