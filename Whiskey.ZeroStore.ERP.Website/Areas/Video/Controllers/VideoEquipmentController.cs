using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.Web.VideoConfig;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Video.Controllers
{
    //[License(CheckMode.Verify)]
    public class VideoEquipmentController : BaseController
    {
        protected readonly IVideoEquipmentContract _videoEquipmentContract;
        protected readonly IVideoJurisdictionContract _videoJurisdictionContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IAdministratorContract _administratorContract;
        public VideoEquipmentController(IVideoEquipmentContract videoEquipmentContract,
            IVideoJurisdictionContract videoJurisdictionContract,
            IMemberContract memberContract,
            IStoreContract storeContract,
            IDepartmentContract departmentContract,
            IAdministratorContract administratorContract)
        {
            _videoEquipmentContract = videoEquipmentContract;
            _videoJurisdictionContract = videoJurisdictionContract;
            _memberContract = memberContract;
            _storeContract = storeContract;
            _departmentContract = departmentContract;
            _administratorContract = administratorContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var count = 0;
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);
            Expression<Func<VideoEquipment, bool>> predicate = FilterHelper.GetExpression<VideoEquipment>(request.FilterGroup);

            List<int> s_List = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            int PageIndex = request.PageCondition.PageIndex + 1;
            var allVideo = _videoEquipmentContract.VideoEquipments.Where(x => s_List.Contains(x.StoreId));
            var da = allVideo
                .Where<VideoEquipment, int>(predicate, request.PageCondition, out count)
                //.Skip(request.PageCondition.PageIndex * request.PageCondition.PageSize).Take(PageIndex * request.PageCondition.PageSize)
                .Select(c => new
                {
                    c.VideoName,
                    c.snNumber,
                    c.Store.StoreName,
                    c.Id,
                    c.CreatedTime,
                    c.Operator.Member.MemberName,
                    c.IsDeleted,
                    c.IsEnabled
                }).ToList();
            List<object> list = new List<object>();
            foreach (var item in da)
            {
                //int useCount = GetUseVideoCount(item.Id);
                var datasource = new
                {
                    VideoName = item.VideoName,
                    snNumber = item.snNumber,
                    StoreName = item.StoreName,
                    useCount = 0,
                    item.Id,
                    channelPicUrl = GetChannelPicUrl(item.snNumber),
                    CreatedTime = item.CreatedTime,
                    MemberName = item.MemberName,
                    isOnline = CheckIsOnline(item.snNumber),
                    item.IsEnabled,
                    item.IsDeleted
                };
                list.Add(datasource);
            }
            GridData<object> data = new GridData<object>(list, count, request.RequestInfo);
            return Json(data);
        }

        public int GetUseVideoCount(int id)
        {
            var list = _videoJurisdictionContract.VideoJurisdictions.Where(x => x.IsDeleted == false &&
             x.IsEnabled == true && x.VideoEquipmentId == id).Select(x => x.MemberId).ToList();
            if (list != null)
            {
                return list.Distinct().Count();
            }
            else
            {
                return 0;
            }
        }

        public string CheckIsOnline(string deviceId)
        {
            string isOnline = "";
            VideoApiHelper api = new VideoApiHelper();
            string result = api.deviceOnline(deviceId);
            try
            {
                dynamic json = JToken.Parse(result) as dynamic;
                if (json.result.code == "0")
                {
                    string isOnlineDesc = json.result.data.onLine;
                    if (isOnlineDesc == "0")
                    {
                        isOnline = "否";
                    }
                    else if (isOnlineDesc == "1")
                    {
                        isOnline = "是";
                    }
                }
            }
            catch (Exception e)
            {


            }
            return isOnline;
        }

        public int CheckIsBind(int Id)
        {
            string snNUmber = _videoEquipmentContract.VideoEquipments.Where(x => x.Id == Id).
                Select(x => x.snNumber).FirstOrDefault();
            VideoApiHelper api = new VideoApiHelper();
            string msg = string.Empty;
            int Identification = 0;
            string json = api.checkDeviceBindOrNot(snNUmber);
            try
            {
                dynamic jsonM = JToken.Parse(json) as dynamic;
                if (jsonM.result.code == "0")
                {
                    if (jsonM.result.data.isBind == "True")
                    {
                        if (jsonM.result.data.isMine == "True")
                        {
                            //设备绑定在当前帐号 
                            Identification = 2;
                            msg = "设备已绑定";
                        }
                        else
                        {
                            Identification = 3;
                            msg = "此设备不能使用,不是绑定在当前账户下的";
                        }
                    }
                    else
                    {
                        //设备未绑定 并且不在线
                        Identification = 1;
                        string onLine = api.deviceOnline(snNUmber);
                        dynamic onLineM = JToken.Parse(onLine) as dynamic;
                        if (onLineM.result.code == "0")
                        {
                            if (onLineM.result.data.onLine == "1")
                            { Identification = 1; }
                            else
                            {
                                Identification = 5;//设备不在线
                            }
                        }
                        else
                        {
                            Identification = 4;
                        }
                    }
                }
                else
                {
                    Identification = 4;
                    msg = "检查当前设备是否绑定出现异常！";
                }
            }
            catch (Exception e)
            {
                Identification = 4;
                msg = "检查当前设备是否绑定出现异常！";
            }
            var data = new
            {
                Identification = Identification,
                msg = msg
            };
            return Identification;
        }

        [HttpGet]
        public ActionResult Create()
        {

            List<SelectListItem> departmentList = new List<SelectListItem>();
            departmentList.Add(new SelectListItem() { Text = "请选择", Value = "" });
            departmentList.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.Department = departmentList;
            return PartialView(new VideoEquipment());
        }
        [HttpPost]
        public ActionResult Create(VideoEquipment ve, string userList)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            string sn_number = ve.snNumber;

            resul = _videoEquipmentContract.Insert(ve);
            if (resul.ResultType == Utility.Data.OperationResultType.Success)
            {
                var config = _videoEquipmentContract.VideoEquipments.Where(x =>
                x.snNumber == sn_number).FirstOrDefault();
                if (config != null)
                {
                    if (!string.IsNullOrEmpty(userList))
                    {
                        string[] userarry = userList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<VideoJurisdiction> list = new List<VideoJurisdiction>();
                        foreach (var item in userarry)
                        {
                            int memberId = 0;
                            Int32.TryParse(item, out memberId);
                            if (memberId > 0)
                            {
                                VideoJurisdiction vj = new VideoJurisdiction() { MemberId = memberId, VideoEquipmentId = config.Id };
                                list.Add(vj);
                            }
                        }
                        _videoJurisdictionContract.Insert(list.ToArray());
                    }
                }
            }

            return Json(resul);
        }

        public ActionResult SearchMember()
        {
            GridRequest request = new GridRequest(Request);
            var searchType = Request["seachType"].ToString();
            var phone = Request["phone"].ToString();
            var Department = Request["Department"].ToString();
            if (searchType == "0")
            {
                var member = _memberContract.Members.Where(x => x.MobilePhone == phone).FirstOrDefault();
                if (member != null)
                {
                    var storeName = _storeContract.Stores.Where(x => x.Id == member.StoreId).Select(x => x.StoreName).FirstOrDefault();
                    var data = new
                    {
                        member.Id,
                        member.MemberName,
                        departmentName = GetDepartmentName(member.Id),
                        storeName
                    };
                    List<object> list = new List<object>();
                    list.Add(data);
                    GridData<object> data1 = new GridData<object>(list, 1, request.RequestInfo);
                    return Json(data1);
                }
                else
                {
                    GridData<object> data1 = new GridData<object>(null, 0, request.RequestInfo);
                    return Json(data1);
                }
            }
            else
            {
                int departmentId = 0;
                Int32.TryParse(Department, out departmentId);
                var allData = _administratorContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled
                && x.DepartmentId == departmentId);
                var da = allData.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
                {
                    c.MemberId,
                    c.Member.MemberName
                }).ToList();
                List<object> list = new List<object>();
                foreach (var item in da)
                {
                    var a = _memberContract.Members.Where(x => !x.IsDeleted && x.IsEnabled
                    && x.Id == item.MemberId).FirstOrDefault();
                    var storeName = _storeContract.Stores.Where(x => x.Id == a.StoreId).Select(x => x.StoreName).FirstOrDefault();
                    if (a != null)
                    {
                        var b = new
                        {
                            a.Id,
                            a.MemberName,
                            departmentName = GetDepartmentName(a.Id),
                            storeName
                        };
                        list.Add(b);
                    }
                }
                GridData<object> data = new GridData<object>(list, allData.Count(), request.RequestInfo);
                return Json(data);
            }

        }

        public string GetDepartmentName(int? memberId)
        {
            var departmentId = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled
            && x.MemberId == memberId).Select(x => x.DepartmentId).FirstOrDefault();
            var departmentName = _departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled
            && x.Id == departmentId).Select(x => x.DepartmentName).FirstOrDefault();
            return departmentName;
        }

        public ActionResult CheckIsExistence(string snNUmber)
        {
            int count = _videoEquipmentContract.VideoEquipments.Where(x =>
             x.snNumber == snNUmber && x.IsDeleted == false && x.IsEnabled == true).Count();
            string msg = string.Empty;
            int Identification = 0;
            if (count == 0 && !string.IsNullOrEmpty(snNUmber))
            {
                //检查设备是否已经绑定
                VideoApiHelper api = new VideoApiHelper();
                try
                {
                    string checkDevice = api.checkDeviceBindOrNot(snNUmber);
                    dynamic json = JToken.Parse(checkDevice) as dynamic;
                    if (json.result.code == "0")
                    {
                        if (json.result.data.isBind == "True")
                        {
                            if (json.result.data.isMine == "True")
                            {
                                //设备绑定在当前帐号 
                                Identification = 2;
                                msg = "设备已绑定";
                            }
                            else
                            {
                                Identification = 3;
                                msg = "此设备不能使用,不是绑定在当前账户下的";
                            }
                        }
                        else
                        {
                            //设备未绑定 并且不在线
                            Identification = 1;
                            msg = "当前设备不在线";
                        }
                    }
                    else
                    {
                        Identification = 4;
                        msg = "检查当前设备是否绑定出现异常！";
                    }
                }
                catch (Exception e)
                {
                    Identification = 4;
                    msg = "出现异常！";
                }
            }
            else
            {
                Identification = 0;
                msg = "设备已存在";
            }

            var data = new
            {
                Identification = Identification,
                msg = msg
            };
            return Json(data);
        }

        public ActionResult Remove(string ids, bool state, int Identification)
        {
            List<int> list = new List<int>();
            VideoApiHelper api = new VideoApiHelper();
            if (!string.IsNullOrEmpty(ids))
            {
                var idArry = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in idArry)
                {
                    int id = 0;
                    Int32.TryParse(item, out id);
                    if (id != 0)
                    {
                        string sn_number = _videoEquipmentContract.VideoEquipments.Where(x => x.Id == id)
    .Select(x => x.snNumber).FirstOrDefault();
                        if (!state)
                        {
                            if (Identification == 1)
                            {
                                try
                                {

                                    string bind = api.bindDevice(sn_number, "");
                                    dynamic json = JToken.Parse(bind) as dynamic;
                                    if (json.result.code == "0")
                                    {
                                        list.Add(id);
                                    }
                                }
                                catch (Exception e)
                                {

                                }
                            }
                            else if (Identification == 2)
                            { list.Add(id); }
                        }
                        else
                        {
                            try
                            {
                                string bind = api.unBindDevice(sn_number);
                                dynamic json = JToken.Parse(bind) as dynamic;
                                if (json.result.code == "0")
                                {
                                    list.Add(id);
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }
            var res = _videoEquipmentContract.Remove(state, list.ToArray());
            return Json(res);
        }

        public ActionResult Disable(string ids, bool state)
        {
            List<int> list = new List<int>();
            if (!string.IsNullOrEmpty(ids))
            {
                var idArry = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in idArry)
                {
                    int id = 0;
                    Int32.TryParse(item, out id);
                    if (id != 0)
                    {
                        list.Add(id);
                    }
                }
            }
            var res = _videoEquipmentContract.Disable(state, list.ToArray());
            return Json(res);
        }

        public ActionResult View(int id)
        {
            var videoEquipment = _videoEquipmentContract.VideoEquipments.Where(x =>
            x.Id == id).FirstOrDefault();
            ViewBag.EquipmentId = id;
            return PartialView(videoEquipment);
        }

        public ActionResult GetUseList()
        {
            GridRequest request = new GridRequest(Request);
            int id = 0;
            string idStr = Request.Form["Id"].ToString();
            Int32.TryParse(idStr, out id);
            var seachVideo = _videoJurisdictionContract.VideoJurisdictions.Where(x => x.VideoEquipmentId == id
            && x.IsDeleted == false && x.IsEnabled == true);

            var da = seachVideo.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
            {
                c.Id,
                c.Member.MemberName,
                c.MemberId,
                OperatorName = c.Operator.Member.MemberName,
                c.CreatedTime,
                c.Member.MobilePhone
            });
            List<object> list = new List<object>();
            foreach (var item in da)
            {
                var member = _memberContract.Members.Where(x => x.Id == item.MemberId).FirstOrDefault();
                var storeName = string.Empty;
                var departmentName = string.Empty;
                if (member != null)
                {
                    storeName = _storeContract.Stores.Where(x => x.Id == member.StoreId).Select(x => x.StoreName).FirstOrDefault();
                    departmentName = GetDepartmentName(member.Id);
                }

                var dataSource = new
                {
                    item.Id,
                    storeName,
                    item.MemberName,
                    item.OperatorName,
                    item.CreatedTime,
                    item.MemberId,
                    item.MobilePhone,
                    departmentName = departmentName
                };
                list.Add(dataSource);
            }
            GridData<object> data = new GridData<object>(list, seachVideo.Count(), request.RequestInfo);
            return Json(data);
        }

        public ActionResult Update(int id)
        {
            var videoEquipment = _videoEquipmentContract.VideoEquipments.Where(x => x.Id == id).FirstOrDefault();
            ViewBag.EquipmentId = id;
            ViewBag.StoreSelect = videoEquipment.StoreId;
            List<SelectListItem> departmentList = new List<SelectListItem>();
            departmentList.Add(new SelectListItem() { Text = "请选择", Value = "" });
            var department = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.DepartmentName
                });
            departmentList.AddRange(department);
            ViewBag.Department = departmentList;
            return PartialView(videoEquipment);
        }

        //添加设备使用者
        public ActionResult AddUser(int memberId, int Id)
        {
            VideoJurisdiction vj = new VideoJurisdiction() { MemberId = memberId, VideoEquipmentId = Id };
            var res = _videoJurisdictionContract.Insert(vj);
            return Json(res);
        }

        //删除设备使用者
        public ActionResult RemoveUser(int id)
        {
            var res = _videoJurisdictionContract.Remove(id);
            return Json(res);
        }

        [HttpPost]
        public ActionResult Update(VideoEquipmentDto ve)
        {
            var model = _videoEquipmentContract.VideoEquipments.Where(x => x.Id == ve.Id).FirstOrDefault();
            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(ve.VideoName))
            {
                if (ve.VideoName != model.VideoName)
                {

                    resul = _videoEquipmentContract.Update(ve);
                }
                else
                {
                    resul = _videoEquipmentContract.Update(ve);
                }
            }
            return Json(resul);
        }

        public ActionResult DownFile()
        {
            string filePath = Server.MapPath("/Content/Video/npZFPlug.exe");
            string fileName = "npZFPlug.exe";
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            Response.Charset = "UTF-8";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            Response.ContentType = "application/octet-stream";

            Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(fileName));
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
            return new EmptyResult();

        }

        public ActionResult ViewMonitor(string sn_Number, string loadtype)
        {
            VideoApiHelper api = new VideoApiHelper();
            ViewBag.token = api.accessToken();
            ViewBag.sn_Number = sn_Number;
            ViewBag.loadtype = loadtype;
            return PartialView();
        }

        public string GetChannelPicUrl(string deviceId)
        {
            //bindDeviceInfo
            VideoApiHelper api = new VideoApiHelper();
            string result = api.bindDeviceInfo(deviceId);
            string channelPicUrl = string.Empty;
            try
            {
                dynamic json = JToken.Parse(result) as dynamic;
                if (json.result.code == "0")
                {
                    channelPicUrl = json.result.data.channels[0].channelPicUrl;
                }
            }
            catch (Exception e)
            {


            }
            return channelPicUrl;
        }

        public ActionResult ViewMonitorIndex()
        {
            return PartialView();
        }
    }
}