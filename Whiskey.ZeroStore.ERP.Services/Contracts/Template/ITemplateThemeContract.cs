using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ITemplateThemeContract : IDependency
    {
        OperationResult Update(params TemplateThemeDto[] dtos);
        IQueryable<TemplateTheme> templateThemes { get; }
        OperationResult Insert(params TemplateThemeDto[] dtos);
        OperationResult Delete(params int[] ids);
        TemplateTheme View(int id);
        OperationResult Disable(params int[] ids);
        OperationResult Enable(params int[] ids);
        OperationResult Remove(params int[] ids);
        OperationResult Recovery(params int[] ids);
        OperationResult SetDefault(int Id);
        TemplateThemeDto Edit(int id);
        bool CheckTemplateName(string themeName, int? Id);
    }
}
