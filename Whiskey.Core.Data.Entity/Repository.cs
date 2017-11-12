
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AutoMapper;

using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;

using Whiskey.Core.Logging;
using Whiskey.Utility.Logging;
using EntityFramework.BulkExtensions;
using System.Reflection;
using System.Dynamic;

namespace Whiskey.Core.Data.Entity
{
    /// <summary>
    /// EntityFramework的仓储实现
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : EntityBase<TKey>
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly ObjectContext _objectContent;
        private readonly DbContext _db;

        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _db = (DbContext)unitOfWork;
            _dbSet = _db.Set<TEntity>();
        }

        /// <summary>
        /// 获取 当前单元操作对象
        /// </summary>
        public IUnitOfWork UnitOfWork { get { return _unitOfWork; } }

        /// <summary>
        /// 获取 当前实体类型的查询数据集
        /// </summary>
        public IQueryable<TEntity> Entities { get { return _dbSet; } }


        /// <summary>
        /// 插入实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>操作影响的行数</returns>
        public int Insert(TEntity entity)
        {
            entity.CheckNotNull("entity");
            _dbSet.Add(entity);
            return SaveChanges();
        }

        /// <summary>
        /// 批量插入实体
        /// </summary>
        /// <param name="entities">实体对象集合</param>
        /// <returns>操作影响的行数</returns>
        public int Insert(IEnumerable<TEntity> entities)
        {
            entities = entities as TEntity[] ?? entities.ToArray();
            _dbSet.AddRange(entities);

            return SaveChanges();
        }

        public OperationResult Insert(ICollection<TEntity> entities, Action<TEntity> checkAction = null)
        {
            entities.CheckNotNull("entities");
            try
            {
                if (checkAction != null)
                {
                    foreach (TEntity entity in entities)
                    {
                        checkAction.Invoke(entity);
                    }
                }
                _dbSet.AddRange(entities);

                int count = SaveChanges();
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{count}个信息添加成功")
                    { Data = entities.Select(c => c.Id).ToArray() }
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception e)
            {
                return new OperationResult(OperationResultType.Error, "插入数据出错，错误如下：" + e.Message);
            }
        }


        /// <summary>
        /// 以DTO为载体批量插入实体
        /// </summary>
        /// <typeparam name="TAddDto">添加DTO类型</typeparam>
        /// <param name="dtos">添加DTO信息集合</param>
        /// <param name="checkAction">添加信息合法性检查委托</param>
        /// <param name="updateFunc">由DTO到实体的转换委托</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert<TAddDto>(ICollection<TAddDto> dtos,
            Action<TAddDto> checkAction = null,
            Func<TAddDto, TEntity, TEntity> updateFunc = null)
            where TAddDto : IAddDto
        {
            dtos.CheckNotNull("dtos");
            List<TEntity> entili = new List<TEntity>();
            foreach (TAddDto dto in dtos)
            {
                TEntity entity = Mapper.Map<TEntity>(dto);
                try
                {
                    if (checkAction != null)
                    {
                        checkAction(dto);
                    }
                    if (updateFunc != null)
                    {
                        entity = updateFunc(dto, entity);
                    }
                }
                catch (Exception e)
                {
                    return new OperationResult(OperationResultType.Error, "插入数据出错，错误如下：" + e.Message);
                }
                entili.Add(entity);
            }
            _dbSet.AddRange(entili);
            int count = SaveChanges();
            return count > 0
                ? new OperationResult(OperationResultType.Success, $"{dtos.Count.ToString()}个信息添加成功", entili.Select(c => c.Id).ToArray())
                : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
        }

        /// 删除实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>操作影响的行数</returns>
        public int Delete(TEntity entity)
        {
            entity.CheckNotNull("entity");
            _dbSet.Remove(entity);
            return SaveChanges();
        }

        public virtual int Delete(TKey key)
        {
            CheckEntityKey(key, "key");
            TEntity entity = _dbSet.Find(key);
            return entity == null ? 0 : Delete(entity);
        }

        /// <summary>
        /// 删除所有符合特定条件的实体
        /// </summary>
        /// <param name="predicate">查询条件谓语表达式</param>
        /// <returns>操作影响的行数</returns>
        public int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            predicate.CheckNotNull("predicate");
            TEntity[] entities = _dbSet.Where(predicate).ToArray();
            return entities.Length == 0 ? 0 : Delete(entities);
        }

        /// <summary>
        /// 批量删除删除实体
        /// </summary>
        /// <param name="entities">实体对象集合</param>
        /// <returns>操作影响的行数</returns>
        public int Delete(IEnumerable<TEntity> entities)
        {
            entities = entities as TEntity[] ?? entities.ToArray();
            _dbSet.RemoveRange(entities);
            return SaveChanges();
        }

        /// <summary>
        /// 以标识集合批量删除实体
        /// </summary>
        /// <param name="ids">标识集合</param>
        /// <param name="checkAction">删除前置检查委托</param>
        /// <param name="deleteFunc">删除委托，用于删除关联信息</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(ICollection<TKey> ids, Action<TEntity> checkAction = null, Func<TEntity, TEntity> deleteFunc = null)
        {
            ids.CheckNotNull("ids");
            List<string> names = new List<string>();
            foreach (TKey id in ids)
            {
                TEntity entity = _dbSet.Find(id);
                try
                {
                    if (checkAction != null)
                    {
                        checkAction(entity);
                    }
                    if (deleteFunc != null)
                    {
                        entity = deleteFunc(entity);
                    }
                }
                catch (Exception e)
                {
                    return new OperationResult(OperationResultType.Error, "删除数据出错，错误如下：" + e.Message);
                }
                _dbSet.Remove(entity);
            }
            int count = SaveChanges();
            return count > 0 ? new OperationResult(OperationResultType.Success, "{0}个信息删除成功".FormatWith(ids.Count)) : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
        }

        /// <summary>
        /// 更新实体对象
        /// </summary>
        /// <param name="entity">更新后的实体对象</param>
        /// <returns>操作影响的行数</returns>
        public int Update(TEntity entity)
        {
            entity.CheckNotNull("entity");
            ((DbContext)_unitOfWork).Update<TEntity, TKey>(entity);
            return SaveChanges();
        }

        public OperationResult Update(ICollection<TEntity> entities, Action<TEntity> checkAction = null)
        {
            entities.CheckNotNull("entities");

            foreach (TEntity entity in entities)
            {
                if (entity == null)
                {
                    return new OperationResult(OperationResultType.QueryNull);
                }
                try
                {
                    checkAction?.Invoke(entity);
                }
                catch (Exception e)
                {
                    return new OperationResult(OperationResultType.Error, "更新数据出错，错误如下：" + e.Message);
                }
            }
            try
            {
                _db.Update<TEntity, TKey>(entities.ToArray());
                int count = SaveChanges();
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{count}个信息更新成功")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// 以DTO为载体批量更新实体
        /// </summary>
        /// <typeparam name="TEditDto">更新DTO类型</typeparam>
        /// <param name="dtos">更新DTO信息集合</param>
        /// <param name="checkAction">更新信息合法性检查委托</param>
        /// <param name="updateFunc">由DTO到实体的转换委托</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update<TEditDto>(ICollection<TEditDto> dtos,
            Action<TEditDto> checkAction = null,
            Func<TEditDto, TEntity, TEntity> updateFunc = null)
            where TEditDto : IEditDto<TKey>
        {
            dtos.CheckNotNull("dtos");
            List<TEntity> entities = new List<TEntity>();
            foreach (TEditDto dto in dtos)
            {
                TEntity entity = _dbSet.Find(dto.Id);
                var create_time = entity.CreatedTime;
                if (entity == null)
                {
                    return new OperationResult(OperationResultType.QueryNull);
                }
                entity = Mapper.Map(dto, entity);
                entity.CreatedTime = create_time;
                try
                {
                    if (checkAction != null)
                    {
                        checkAction(dto);
                    }
                    if (updateFunc != null)
                    {
                        entity = updateFunc(dto, entity);
                    }
                }
                catch (Exception e)
                {
                    return new OperationResult(OperationResultType.Error, "更新数据出错，错误如下：" + e.Message);
                }
                entities.Add(entity);
            }
            try
            {
                _db.Update<TEntity, TKey>(entities.ToArray());
                int count = SaveChanges();
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{dtos.Count.ToString()}个信息更新成功")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 实体存在性检查
        /// </summary>
        /// <param name="predicate">查询条件谓语表达式</param>
        /// <param name="id">编辑的实体标识</param>
        /// <returns>是否存在</returns>
        public bool ExistsCheck(Expression<Func<TEntity, bool>> predicate, TKey id = default(TKey))
        {
            TKey defaultId = default(TKey);

            var entity = _dbSet.Where(predicate).Select(m => new { m.Id }).FirstOrDefault();

            if (entity == null)
            {
                return false;
            }
            else if (!id.Equals(defaultId) && !entity.Id.Equals(id))
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 查找指定主键的实体
        /// </summary>
        /// <param name="key">实体主键</param>
        /// <returns>符合主键的实体，不存在时返回null</returns>
        public TEntity GetByKey(TKey key)
        {
            CheckEntityKey(key, "key");
            return _dbSet.Find(key);
        }

        /// <summary>
        /// 获取贪婪加载导航属性的查询数据集
        /// </summary>
        /// <param name="path">属性表达式，表示要贪婪加载的导航属性</param>
        /// <returns>查询数据集</returns>
        public IQueryable<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            path.CheckNotNull("path");
            return _dbSet.Include(path);
        }

        /// <summary>
        /// 获取贪婪加载多个导航属性的查询数据集
        /// </summary>
        /// <param name="paths">要贪婪加载的导航属性名称数组</param>
        /// <returns>查询数据集</returns>
        public IQueryable<TEntity> Includes(params string[] paths)
        {
            paths.CheckNotNull("paths");
            IQueryable<TEntity> source = _dbSet;
            foreach (string path in paths)
            {
                source = source.Include(path);
            }
            return source;
        }

        ///// <summary>
        ///// 执行SQL语句或存储过程，返回记录行数
        ///// </summary>
        ///// <param name="commandText">SQL语句</param>
        ///// <param name="parameters">参数</param>
        ///// <returns>受影响的记录数</returns>
        //public int ExecuteStoreCommand(string commandText, params object[] parameters)
        //{
        //    commandText.CheckNotNullOrEmpty("commandText");
        //    parameters.CheckNotNullOrEmpty("parameters");
        //    return _objectContent.ExecuteStoreCommand(commandText, parameters);
        //}

        ///// <summary>
        ///// 执行SQL语句或存储过程，返回实体记录集合
        ///// </summary>
        ///// <typeparam name="T">实体对象</typeparam>
        ///// <param name="commandText">SQL语句</param>
        ///// <param name="parameters">参数</param>
        ///// <returns>实体集合</returns>
        //public ObjectResult<T> ExecuteStoreQuery<T>(string commandText, params object[] parameters)
        //{
        //    commandText.CheckNotNullOrEmpty("commandText");
        //    parameters.CheckNotNullOrEmpty("parameters");
        //    return _objectContent.ExecuteStoreQuery<T>(commandText, parameters);
        //}




#if NET45

        /// <summary>
        /// 异步插入实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> InsertAsync(TEntity entity)
        {
            entity.CheckNotNull("entity");
            _dbSet.Add(entity);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 异步批量插入实体
        /// </summary>
        /// <param name="entities">实体对象集合</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> InsertAsync(IEnumerable<TEntity> entities)
        {
            entities = entities as TEntity[] ?? entities.ToArray();
            _dbSet.AddRange(entities);

            return await SaveChangesAsync();
        }

        /// <summary>
        /// 异步删除实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> DeleteAsync(TEntity entity)
        {
            entity.CheckNotNull("entity");
            _dbSet.Remove(entity);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 异步删除指定编号的实体
        /// </summary>
        /// <param name="key">实体编号</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> DeleteAsync(TKey key)
        {
            CheckEntityKey(key, "key");
            TEntity entity = await _dbSet.FindAsync(key);
            return entity == null ? 0 : await DeleteAsync(entity);
        }

        /// <summary>
        /// 异步删除所有符合特定条件的实体
        /// </summary>
        /// <param name="predicate">查询条件谓语表达式</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            predicate.CheckNotNull("predicate");
            TEntity[] entities = await _dbSet.Where(predicate).ToArrayAsync();
            return entities.Length == 0 ? 0 : await DeleteAsync(entities);
        }

        /// <summary>
        /// 异步批量删除删除实体
        /// </summary>
        /// <param name="entities">实体对象集合</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> DeleteAsync(IEnumerable<TEntity> entities)
        {
            entities = entities as TEntity[] ?? entities.ToArray();
            _dbSet.RemoveRange(entities);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 异步更新实体对象
        /// </summary>
        /// <param name="entity">更新后的实体对象</param>
        /// <returns>操作影响的行数</returns>
        public async Task<int> UpdateAsync(TEntity entity)
        {
            entity.CheckNotNull("entity");
            ((DbContext)_unitOfWork).Update<TEntity, TKey>(entity);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 异步查找指定主键的实体
        /// </summary>
        /// <param name="key">实体主键</param>
        /// <returns>符合主键的实体，不存在时返回null</returns>
        public async Task<TEntity> GetByKeyAsync(TKey key)
        {
            CheckEntityKey(key, "key");
            return await _dbSet.FindAsync(key);
        }

#endif

        #region 私有方法

        private int SaveChanges()
        {
            #region 事务模式

            //if (!_unitOfWork.TransactionEnabled)
            //{
            //    return _unitOfWork.SaveChanges();
            //}
            //else
            //{
            //    using (var trans = this.GetTransaction())
            //    {
            //        var opCount = 0;
            //        try
            //        {
            //            opCount = _unitOfWork.SaveChanges();
            //            trans.Commit();
            //        }
            //        catch
            //        {
            //            trans.Rollback();
            //            throw;
            //        }
            //        return opCount;
            //    }
            //}

            #endregion

            return _unitOfWork.TransactionEnabled ? 0 : _unitOfWork.SaveChanges();
        }

#if NET45

        private async Task<int> SaveChangesAsync()
        {
            return _unitOfWork.TransactionEnabled ? 0 : await _unitOfWork.SaveChangesAsync();
        }

#endif

        private static void CheckEntityKey(object key, string keyName)
        {
            key.CheckNotNull("key");
            keyName.CheckNotNull("keyName");

            Type type = key.GetType();
            if (type == typeof(int))
            {
                ((int)key).CheckGreaterThan(keyName, 0);
            }
            else if (type == typeof(string))
            {
                ((string)key).CheckNotNullOrEmpty(keyName);
            }
            else if (type == typeof(Guid))
            {
                ((Guid)key).CheckNotEmpty(keyName);
            }
        }


        #endregion


        public OperationResult Update(ICollection<TEntity> entity)
        {
            try
            {
                ((DbContext)_unitOfWork).Update<TEntity, TKey>(entity.ToArray());
                int count = SaveChanges();
                return count > 0
                    ? new OperationResult(OperationResultType.Success, count + "条数据受影响")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DbContextTransaction GetTransaction()
        {
            try
            {
                return ((DbContext)_unitOfWork).Database.BeginTransaction();

            }
            catch (Exception)
            {
                throw;
            }
        }

        #region 批量操作

        public OperationResult InsertBulk(IEnumerable<TEntity> entities, Action<TEntity> updateAction = null)
        {
            entities.CheckNotNull("entities");
            try
            {
                if (updateAction != null)
                {
                    foreach (TEntity entity in entities)
                    {
                        updateAction.Invoke(entity);
                    }
                }
                int count = _db.BulkInsert(entities, InsertOptions.OutputIdentity);
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{count}个信息添加成功")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public OperationResult UpdateBulk(IEnumerable<TEntity> entities, Action<TEntity> updateAction = null)
        {
            entities.CheckNotNull("entities");
            try
            {
                if (updateAction != null)
                {
                    foreach (TEntity entity in entities)
                    {
                        updateAction.Invoke(entity);
                    }
                }

                int count = _db.BulkUpdate(entities, UpdateOptions.Default);
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{count}个信息更新成功")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public OperationResult DeleteBulk(IEnumerable<TEntity> entities)
        {
            entities.CheckNotNull("entities");
            try
            {
                int count = _db.BulkDelete(entities);
                return count > 0
                    ? new OperationResult(OperationResultType.Success, $"{count}个信息删除成功")
                    : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

    }
}