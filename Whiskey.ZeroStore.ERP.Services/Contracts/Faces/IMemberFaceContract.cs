using System.IO;
using System.Linq;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberFaceContract : IBaseContract<MemberFace>
    {
        /// <summary>
        /// 根据人脸图像匹配到的会员,可能为空
        /// </summary>
        IQueryable<Member> SearchedMembers(int storeId, string imgUrl, Stream imgStream = null);

        /// <summary>
        /// 会员脸部图像对比,imgUrl和imgStream至少传一个
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="imgUrl"></param>
        /// <param name="imgStream"></param>
        /// <returns></returns>
        OperationResult CompareFace(int MemberId, string imgUrl, Stream imgStream = null);
        /// <summary>
        /// 获取会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        OperationResult GetFace(int MemberId, int Count = 3);
        /// <summary>
        /// 移除会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="FaceToken"></param>
        /// <returns></returns>
        OperationResult RemoveFace(int MemberId, string FaceToken);
        /// <summary>
        /// 移除会员所有人脸图像信息
        /// </summary>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        OperationResult RemoveFaceAll(int MemberId);
        /// <summary>
        /// 添加会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="imgUrl"></param>
        /// <returns></returns>
        OperationResult AddFace(int MemberId, string imgUrl, Stream imgStream = null);
        /// <summary>
        /// 根据人脸图像匹配到的会员Ids
        /// </summary>
        /// <param name="storeId">店铺Id</param>
        /// <param name="imgUrl"></param>
        /// <param name="imgStream"></param>
        /// <returns>ResultType=Success时,Data为List<int>会员Ids</returns>
        OperationResult SearchMemberIds(int storeId, string imgUrl, Stream imgStream = null);
        /// <summary>
        /// 移动会员人脸信息到新的店铺
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="NewStoreId"></param>
        /// <returns></returns>
        OperationResult MoveMemberToNewFaceSet(int MemberId, int? NewStoreId);

    }
}
