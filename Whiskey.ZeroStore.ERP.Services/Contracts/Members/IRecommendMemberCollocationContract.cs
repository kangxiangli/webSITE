
using System.Collections.Generic;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IRecommendMemberCollocationContract : IBaseContract<RecommendMemberCollocation>
    {

        OperationResult Delete(params RecommendMemberCollocation[] entities);

        OperationResult Update(ICollection<RecommendMemberCollocation> entities);

        RecommendMemberCollocation Edit(int id);

    }


    public interface IRecommendMemberSingleProductContract : IBaseContract<RecommendMemberSingleProduct>
    {

        OperationResult Delete(params RecommendMemberSingleProduct[] entities);

        OperationResult Update(ICollection<RecommendMemberSingleProduct> entities);

        RecommendMemberSingleProduct Edit(int id);


        OperationResult SaveMemberId(string bigProdNumber, params SaveMemberRecommendEntry[] recommendMembers);

    }








}

