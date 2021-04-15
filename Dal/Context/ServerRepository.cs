using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dal.Context
{
    public class ServerRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {

        #region 参数
        private readonly ServerContext _context;
        private DbSet<TEntity> _entities;
        #endregion

        #region 构造函数
        public ServerRepository(ServerContext context)
        {
            this._context = context;
            this._entities = _context.Set<TEntity>();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 实体更改的回滚并返回完整的错误消息
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Error message</returns>
        protected string GetFullErrorTextAndRollbackEntityChanges(DbUpdateException exception)
        {
            //回滚实体
            if (_context is DbContext dbContext)
            {
                var entries = dbContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).ToList();

                entries.ForEach(entry => entry.State = EntityState.Unchanged);
            }
            _context.SaveChanges();
            return exception.ToString();
        }
        #endregion

        #region 方法

        public virtual TEntity Load(TKey key)
        {
            return Entities.Find(key);
        }
        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return Entities.AsNoTracking().FirstOrDefault(expression);
        }

        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            return Entities.AsNoTracking().Where(expression);
        }
        public virtual IEnumerable<TEntity> FindAll()
        {
            return Entities.AsNoTracking().AsQueryable();
            //return Entities.Find(id);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                Entities.Add(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                Entities.AddRange(entities);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                Entities.Update(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(TEntity entity, Expression<Func<TEntity, object>> includeProperties)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _context.Attach(entity);
                NewExpression newExpr = includeProperties.Body as NewExpression;
                for (var i = 0; i < newExpr.Members.Count; i++)
                {
                    string name = newExpr.Members[i].Name;
                    _context.Entry(entity).Property(name).IsModified = true;
                }
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Update(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                Entities.UpdateRange(entities);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> includeProperties)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                NewExpression newExpr = includeProperties.Body as NewExpression;

                foreach (var entity in entities)
                {
                    _context.Attach(entity);
                    for (var i = 0; i < newExpr.Members.Count; i++)
                    {
                        string name = newExpr.Members[i].Name;
                        _context.Entry(entity).Property(name).IsModified = true;
                    }
                }
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }
        public virtual void Delete(TKey key)
        {
            try
            {
                TEntity entity = Entities.Find(key);
                Entities.Remove(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                Entities.Attach(entity);
                Entities.Remove(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                Entities.AttachRange(entities);
                Entities.RemoveRange(entities);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="expression">表达式</param>
        public virtual void Delete(Expression<Func<TEntity, bool>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("表达式不可为空！");

            try
            {
                var entity = Entities.Where(expression);
                Entities.RemoveRange(entity);
                _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }

        public virtual bool Exists(Expression<Func<TEntity, bool>> expression)
        {
            return Entities.AsNoTracking().Count(expression) > 0;
        }
        #endregion

        #region 查询数据集
        public virtual IQueryable<TEntity> Table => Entities;

        /// <summary>
        /// 获取一个启用“no tracking”(EF特性)的表，仅当您仅为只读操作加载记录时才使用它
        /// </summary>
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

        protected virtual DbSet<TEntity> Entities
        {
            get
            {
                return _entities;
            }
        }
        public virtual IDbContext DbContext
        {
            get { return this._context; }
        }
        #endregion

        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual bool Commit(Action action)
        {
            try
            {
                return _context.Commit(action);
            }
            catch (DbUpdateException exception)
            {
                //ensure that the detailed error text is saved in the Log
                throw new Exception(GetFullErrorTextAndRollbackEntityChanges(exception), exception);
            }
        }
        #region Sequence Method
        /// <summary>
        /// 生成下一主键值
        /// </summary>
        /// <param name="tableName">表名，默认为 typeof(T).Name</param>
        /// <returns></returns>
        //protected TKey GenerateNextKeyValue(string tableName = null)
        //{
        //    return _context.Database.SqlQueryScalar<TKey>("SELECT S" + (tableName ?? typeof(TEntity).Name) + ".nextval FROM dual");
        //}

        //protected List<TKey> GenerateNextKeyValues(string tableName = null, int count = 1)
        //{
        //    if (count <= 0) return new List<TKey>();

        //    return _context.Database.SqlQuery<TKey>("SELECT S" + (tableName ?? typeof(TEntity).Name) + ".nextval FROM dual CONNECT BY rownum <= " + count);
        //}
        #endregion
    }
}
