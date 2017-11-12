
using System.Collections.Generic;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ISizeExtentionContract : IBaseContract<SizeExtention, SizeExtentionDto>
    {
        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <returns></returns>
        List<SelectListItem> SelectListItem(bool hasHit = false);
    }
}

