using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Web.Helper;
using System.Text.RegularExpressions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Security.Cryptography;
using System.Net;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.MobileApi.Areas.Members.Models;


namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class AddressController : Controller
    {

        #region 初始化业务层操作对象
        
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AddressController));
        
        //声明业务层操作对象        
        protected readonly IMemberAddressContract _memberAddressContract;

        protected readonly IAreaItemContract _areaItemContract;
         
        //构造函数-初始化业务层操作对象
        public AddressController(IMemberAddressContract memberAddressContract
            ,IAreaItemContract areaItemContract)
        {
            _memberAddressContract = memberAddressContract;
            _areaItemContract = areaItemContract;
        }
        #endregion

        #region 添加会员收货地址
        /// <summary>
        /// 添加会员收货地址
        /// </summary>
        /// <returns></returns>
        public JsonResult Add(M_Address address)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            oper = this.CheckParamter(address);
            if (oper.ResultType == OperationResultType.Success)
            {
                MemberAddressDto dto = oper.Data as MemberAddressDto;
                oper = _memberAddressContract.Insert(dto);
                return Json(oper);
            }
            else
            {
                return Json(oper);
            }            
        }

        #endregion

        #region 校验参数
        /// <summary>
        /// 校验参数
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        
        private OperationResult CheckParamter(M_Address address)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "添加数据错误");
            try
            {

                MemberAddressDto mAddress = new MemberAddressDto();
                #region 校验参数
                if (string.IsNullOrEmpty(address.ProvinceId))
                {
                    oper.Message = "请选择省份";
                    return oper;
                }
                else
                {
                    mAddress.ProvinceId = int.Parse(address.ProvinceId);
                }
                if (string.IsNullOrEmpty(address.CityId))
                {
                    oper.Message = "请选择城市";
                    return oper;
                }
                else
                {
                    mAddress.CityId = int.Parse(address.CityId);
                }
                if (string.IsNullOrEmpty(address.CountyId))
                {
                    oper.Message = "请选择县（区）";
                    return oper;
                }
                else
                {
                    mAddress.CountyId = int.Parse(address.CountyId);
                }
                if (string.IsNullOrEmpty(address.Receiver))
                {
                    oper.Message = "请选择收件人";
                    return oper;
                }
                else
                {
                    string strReceiver = address.Receiver.Trim();
                    if (strReceiver.Length > 25)
                    {
                        oper.Message = "收件人名称不能超过20个字符";
                        return oper;
                    }
                    else
                    {
                        mAddress.Receiver = strReceiver;
                    }
                }
                if (string.IsNullOrEmpty(address.HomeAddress))
                {
                    oper.Message = "请填写详细地址";
                    return oper;
                }
                else
                {
                    string strHomeAddress = address.HomeAddress.Trim();
                    if (strHomeAddress.Length > 50)
                    {
                        oper.Message = "详细地址不能超过50个字符";
                        return oper;
                    }
                    else
                    {
                        mAddress.HomeAddress = strHomeAddress;
                    }
                }
                if (!string.IsNullOrEmpty(address.Telephone))
                {
                    string strTelephone = address.Telephone.Trim();
                    if (strTelephone.Length > 13)
                    {
                        oper.Message = "固定电话不能超过13个字符";
                        return oper;
                    }
                    else
                    {
                        mAddress.Telephone = strTelephone;
                    }
                }
                if (string.IsNullOrEmpty(address.MobilePhone))
                {
                    oper.Message = "请填写手机号码";
                    return oper;
                }
                else
                {
                    string strMobilePhone = address.MobilePhone.Trim();
                    if (strMobilePhone.Length > 11)
                    {
                        oper.Message = "固定电话不能超过13个字符";
                        return oper;
                    }
                    else
                    {
                        mAddress.MobilePhone = strMobilePhone;
                    }
                }
                if (string.IsNullOrEmpty(address.ZipCode))
                {
                    oper.Message = "请填写邮政编码";
                    return oper;
                }
                else
                {
                    string strZipCode = address.ZipCode.Trim();
                    if (strZipCode.Length > 6)
                    {
                        oper.Message = "邮政编码不能超过6个字符";
                        return oper;
                    }
                    else
                    {
                        mAddress.ZipCode = strZipCode;
                    }
                }
                if (address.IsDefault != (int)DefaultFlag.Yes)
                {
                    mAddress.IsDefault = true;
                }
                else if (address.IsDefault != (int)DefaultFlag.No)
                {
                    mAddress.IsDefault = false;
                }
                else
                {
                    oper.Message = "参数异常";
                    return oper;
                }
                #endregion
                oper.ResultType = OperationResultType.Success;
                oper.Data = address;
                return oper;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return oper;
            }
        }
        #endregion

        #region 获取地址列表
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList(int PageIndex=1,int PageSize=10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error,"服务器忙,请稍后重试");
            try
            {
                string strMemberId= Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<MemberAddress> listAddress= _memberAddressContract.MemberAddresss.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId==memberId);
                listAddress = listAddress.OrderByDescending(x => x.IsDefault).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                var data = listAddress.Select(x => new
                {
                    AddressId = x.Id,
                    Address = x.Province.AreaName + x.City.AreaName + x.County.AreaName + x.HomeAddress,
                    x.Receiver,
                    x.MobilePhone,
                    x.IsDefault
                });
                oper.ResultType=OperationResultType.Success;
                oper.Message=string.Empty;
                oper.Data=data;
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);
            }           
        }
        #endregion

        #region 设为默认地址
        public JsonResult SetDefault()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙,请稍后重试");
            try
            {
                string strMemberId = Request["MemberId"];
                string strAddressId = Request["AddressId"];
                int memberId = int.Parse(strMemberId);
                int addressId = int.Parse(strAddressId);                
                oper = _memberAddressContract.SetDefault(memberId, addressId);
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);
            }
        }
        #endregion

        #region 获取编辑数据
        public JsonResult GetEdit()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙,请稍后重试");
            try
            {
                string strMemberId = Request["MemberId"];
                string strAddressId = Request["AddressId"];
                int memberId = int.Parse(strMemberId);
                int addressId = int.Parse(strAddressId);
                MemberAddress address = _memberAddressContract.View(addressId);
                oper.ResultType=OperationResultType.Success;
                oper.Message=string.Empty;
                oper.Data=address;
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);
            }
        }
        #endregion

        #region 保存编辑
        public JsonResult SaveEdit(M_Address address)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            oper = this.CheckParamter(address);
            if (oper.ResultType == OperationResultType.Success)
            {
                MemberAddressDto dto = oper.Data as MemberAddressDto;
                oper = _memberAddressContract.Update(dto);
                return Json(oper);
            }
            else
            {
                return Json(oper);
            } 
        }
        #endregion

        #region 省市县三级联动
        /// <summary>
        /// 省市县三级联动
        /// </summary>
        /// <returns></returns>
        public JsonResult GetArea()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            try
            {
                string strProvinceId = Request["ProvinceId"];
                string strCityId = Request["CityId"];
                IQueryable<AreaItem> list = _areaItemContract.AreaItems.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                if (string.IsNullOrEmpty(strCityId))
                {
                    oper = this.Areas(list, strProvinceId);
                    return Json(oper);
                }
                else
                {
                    oper = this.Areas(list, strCityId);
                    return Json(oper);
                }
                
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);                
            }
        }

        #region 获取联动数据
        /// <summary>
        /// 获取联动数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="strId"></param>
        /// <returns></returns>
        private OperationResult Areas(IQueryable<AreaItem> list,string strId)
        {
            int? id=null;
            if (!string.IsNullOrEmpty(strId))
	        {
	        	 id=int.Parse(strId);
	        }
            var data = list.Where(x => x.ParentId == id).ToList().Select(x => new
            {
                CountyId = x.Id,
                x.AreaName,
            });
            return new OperationResult(OperationResultType.Success, string.Empty, data);
        }
        #endregion
        #endregion
    }
}