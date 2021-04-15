using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dal.Context
{
    public class DynamicSqlQuery
    {
        #region Factory Method
        /// <summary>
        /// 创建初始动态拼接SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DynamicSqlQuery Create(string sql, params object[] parameters)
        {
            return new DynamicSqlQuery().Append(sql, parameters);
        }

        /// <summary>
        /// 创建初始动态拼接SQL查询，并添加换行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DynamicSqlQuery CreateLine(string sql, params object[] parameters)
        {
            return new DynamicSqlQuery().Append(sql + Environment.NewLine, parameters);
        }

        #endregion

        #region Fields & Constructor
        private static readonly Regex _placeholder = new Regex(@"(?<S>[^\{]?)\{\s*(?<Place>\d+)s*\}(?<E>[^\}]?)", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly StringBuilder _sql = new StringBuilder();
        private readonly List<object> _parameters = new List<object>();
        private readonly Lazy<object[]> _lazyParameters;

        /// <summary>
        /// 构造
        /// </summary>
        public DynamicSqlQuery() { _lazyParameters = new Lazy<object[]>(_parameters.ToArray); }

        #endregion

        #region Properties
        /// <summary>
        /// 获取查询SQL语句
        /// </summary>
        public string Sql { get { return _sql.ToString(); } }
        /// <summary>
        /// 获取参数集合清单
        /// </summary>
        public object[] Parameters { get { return _lazyParameters.Value; } }

        /// <summary>
        /// 判定是否为空SQL
        /// </summary>
        public bool IsEmpty { get { return _sql.Length == 0; } }
        #endregion

        #region Methods

        /// <summary>
        /// 添加参数绑定并在后面添加换行
        /// </summary>
        /// <param name="sqlSnippet"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLine(string sqlSnippet, params object[] parameters)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            return Append(sqlSnippet + Environment.NewLine, parameters);
        }

        /// 添加Sql片断并在后面添加换行
        /// </summary>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLine(string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            return Append(sqlSnippet + Environment.NewLine);
        }
        /// <summary>
        /// 添加SQL片断
        /// </summary>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery Append(string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");
            _sql.Append(sqlSnippet);
            return this;
        }

        /// <summary>
        /// 添加SQL片断并绑定参数 {n}
        /// </summary>
        /// <param name="sqlSnippet"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DynamicSqlQuery Append(string sqlSnippet, params object[] parameters)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (_lazyParameters.IsValueCreated)
                throw new InvalidOperationException("参数集已生成后无法再追加SQL片段，请确保在所有SQL拼接完成后，使用其 Parameters.");

            var placeholders = _placeholder.Matches(sqlSnippet);

            if (placeholders.Count == 1)
            {
                _sql.AppendFormat(sqlSnippet, "{" + _parameters.Count + "}");
                _parameters.Add(parameters[0]);
            }
            else if (placeholders.Count > 1)
            {
                var placeMap = new SortedList<int, object>();
                foreach (Match placeholder in placeholders)
                {
                    var place = int.Parse(placeholder.Groups["Place"].Value);
                    if (place < 0 || place >= parameters.Length)
                        throw new IndexOutOfRangeException(placeholder.Value + "超出参数值范围");
                    placeMap[place] = parameters[place];
                }

                // 按序列生成参数占位
                _sql.AppendFormat(sqlSnippet, placeMap.Keys.Select(_ => "{" + (_parameters.Count + _) + "}").ToArray());
                _parameters.AddRange(placeMap.Values);
            }
            else
            {
                _sql.Append(sqlSnippet);
            }
            return this;
        }
        /// <summary>
        /// 当 val 非null时，添加 sqlSnippet 到SQL语句中，并将 val 参数绑定到 {0} 占位， 并在后面添加换行
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLineNotNull(object val, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            return AppendNotNull(val, sqlSnippet + Environment.NewLine);
        }

        /// <summary>
        /// 当 val 非null时，添加 sqlSnippet 到SQL语句中，并将 val 参数绑定到 {0} 占位
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendNotNull(object val, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            if (val != null)
                return Append(sqlSnippet, val);
            return this;
        }

        /// <summary>
        /// 当 val 非空串或null时，添加 sqlSnippet 到SQL语句中，并将 val 参数绑定到 {0} 占位， 并在后面添加换行
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLineNotNullOrEmpty(string val, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            return AppendNotNullOrEmpty(val, sqlSnippet + Environment.NewLine);
        }

        /// <summary>
        /// 当 val 非空串或null时，添加 sqlSnippet 到SQL语句中，并将 val 参数绑定到 {0} 占位
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendNotNullOrEmpty(string val, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            if (!string.IsNullOrEmpty(val))
                return Append(sqlSnippet, val);
            return this;
        }

        /// <summary>
        /// 只有当 condition 为真是，才添加 sqlSnippet及参数，并添加换行符
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sqlSnippet"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLineIf(bool condition, string sqlSnippet, params object[] parameters)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");
            if (condition)
            {
                return Append(sqlSnippet + Environment.NewLine, parameters);
            }
            return this;
        }

        /// <summary>
        /// 只有当 condition 为真是，才添加 sqlSnippet，并添加换行符
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLineIf(bool condition, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");
            if (condition)
            {
                return AppendLine(sqlSnippet);
            }
            return this;
        }

        /// <summary>
        /// 只有当 condition 为真是，才添加 sqlSnippet及参数
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sqlSnippet"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendIf(bool condition, string sqlSnippet, params object[] parameters)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");
            if (condition)
            {
                return Append(sqlSnippet, parameters);
            }
            return this;
        }

        /// <summary>
        /// 只有当 condition 为真是，才添加 sqlSnippet
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendIf(bool condition, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");
            if (condition)
            {
                return Append(sqlSnippet);
            }
            return this;
        }

        /// <summary>
        /// 如果给定values非空， 添加IN代码片断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendIn<T>(IList<T> values, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            if (values != null && values.Count > 0)
            {
                if (values.Count > 1000)
                    throw new InvalidOperationException("IN 检索范围超出最大限制1000.");

                sqlSnippet = _placeholder.Replace(sqlSnippet, match =>
                {
                    if (match.Groups["Place"].Value != "0")
                        throw new InvalidOperationException("仅支持 {0} 的参数占位.");
                    var start = _parameters.Count;
                    _parameters.AddRange(values.Cast<object>());
                    return match.Groups["S"].Value +
                                string.Join(", ", values.Select((_, i) => "{" + (start + i) + "}"))
                            + match.Groups["E"].Value;
                });
                _sql.Append(sqlSnippet);
            }

            return this;
        }

        /// <summary>
        /// 如果给定values非空， 添加IN代码片断, 并添加换行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="sqlSnippet"></param>
        /// <returns></returns>
        public DynamicSqlQuery AppendLineIn<T>(IList<T> values, string sqlSnippet)
        {
            if (sqlSnippet == null)
                throw new ArgumentNullException("sqlSnippet");

            return AppendIn<T>(values, sqlSnippet + Environment.NewLine);
        }
        #endregion
    }
}
