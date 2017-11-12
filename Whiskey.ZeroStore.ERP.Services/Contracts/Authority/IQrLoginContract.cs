using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IQrLoginContract : IDependency
    {
        IQueryable<QrLogin> QrLogins { get; }

        QrLogin View(int Id);

        QrLoginDto Edit(int Id);

        OperationResult Delete(params int[] ids);

        Task<int> DeleteAsync(Expression<Func<QrLogin, bool>> predicate);

        OperationResult Delete(Expression<Func<QrLogin, bool>> predicate);

        OperationResult Update(params QrLoginDto[] dtos);

        OperationResult Update(params QrLogin[] qrlogin);

        OperationResult Insert(params QrLoginDto[] dtos);

        OperationResult Insert(params QrLogin[] qrlogin);
    }
}
