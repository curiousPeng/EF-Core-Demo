using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dal.Context
{
    public partial interface IRepository<TEntity, TKey> where TEntity : class
    {
        #region 方法
        TEntity Load(TKey key);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression);
        IEnumerable<TEntity> FindAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> expression);
        void Insert(TEntity entity);
        void Insert(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Update(TEntity entity, Expression<Func<TEntity, object>> includeProperties);
        void Update(IEnumerable<TEntity> entities);
        void Update(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> includeProperties);
        void Delete(TEntity entity);
        void Delete(TKey key);
        void Delete(IEnumerable<TEntity> entities);
        void Delete(Expression<Func<TEntity, bool>> expression);

        bool Exists(Expression<Func<TEntity, bool>> expression);
        #endregion

        #region 查询数据集
        IQueryable<TEntity> Table { get; }

        /// <summary>
        /// 获取一个启用“no tracking”(EF特性)的表，仅当您仅为只读操作加载记录时才使用它
        /// </summary>
        IQueryable<TEntity> TableNoTracking { get; }

        IDbContext DbContext { get; }
        #endregion

        /// <summary>
        /// 提交事务
        /// </summary>
        bool Commit(Action action);

        //执行存储过程

        //执行视图

        //执行手写返回实体类Sql语句

        //执行无返回Sql语句
    }
}
