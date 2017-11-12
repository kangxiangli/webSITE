using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ITrainintBlogContract:IBaseContract<TrainingBlogEntity>
    {
        OperationResult Delete(params TrainingBlogEntity[] entities);
        OperationResult Update(ICollection<TrainingBlogEntity> entities);
        TrainingBlogEntity Edit(int id);


    }


}
