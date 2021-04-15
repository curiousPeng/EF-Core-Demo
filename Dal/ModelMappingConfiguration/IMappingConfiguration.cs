using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dal.ModelMappingConfiguration
{
    /// <summary>
    /// 表示数据库上下文模型映射配置
    /// </summary>
    public partial interface IMappingConfiguration
    {
        /// <summary>
        /// 应用此映射配置
        /// </summary>
        /// <param name="modelBuilder">用于构造数据库上下文模型的生成器</param>
        void ApplyConfiguration(ModelBuilder modelBuilder);
    }
}
