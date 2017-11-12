using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.MobileApi.Controllers.Approvals.Controllers
{
    public class ApprovalController : ApiController
    {

         #region 初始化业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApprovalController));
        //声明业务层操作对象
        protected readonly IMemberCollocationContract _memberCollocationContract;

        protected readonly IApprovalContract _productApprovalEleContract;
         
        //构造函数-初始化业务层操作对象
        public ApprovalController(IMemberCollocationContract memberCollocationContract,
            IApprovalContract productApprovalEleContract)
        {
            _memberCollocationContract = memberCollocationContract;
            _productApprovalEleContract = productApprovalEleContract;
        }
        #endregion

        #region 添加赞
        public OperationResult Add([FromBody]ApprovalDto dto)
        {
           var result = _productApprovalEleContract.Insert(dto);
           return result;
        }
        #endregion


        // GET: api/Approval         
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Approval/5
         
        public string Get(int id)
        {
            return "value";
        }


        public string PostList([FromBody]string value)
        {
            return value;
        }

        //// POST: api/Approval
        //public void Post([FromBody]SizeInfo value)
        //{
        //    int k = 0;
        //}

        // PUT: api/Approval/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Approval/5
        public void Delete(int id)
        {
        }
    }
}
