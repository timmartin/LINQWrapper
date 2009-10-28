using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using LINQWrapper.DBMapping;
using LINQWrapper.Exceptions;
using LINQWrapper.SQLExpressions;

namespace LINQWrapper
{
    /// <summary>
    /// Builds SQL strings for the MySQL dialect
    /// </summary>
    public class MySQLBuilder : SQLBuilder
    {
        public MySQLBuilder()
        {
            selectExpressions = new List<string>();
            fromExpressions = new List<string>();
            joins = new List<JoinDetails>();
            orderExpressions = new List<OrderExpression>();
            countQuery = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This needs further testing / investigation. In particular, I'm not sure about the aliasing of the
        /// whereConstraint and orderExpressions members
        /// </remarks>
        /// <returns></returns>
        public MySQLBuilder Clone()
        {
            return new MySQLBuilder()
            {
                selectExpressions = new List<string>(this.selectExpressions),
                fromExpressions = new List<string>(this.fromExpressions),
                joins = new List<JoinDetails>(this.joins),
                whereConstraint = this.whereConstraint,
                orderExpressions = new List<OrderExpression>(this.orderExpressions),
                skipResults = this.skipResults,
                takeResults = this.takeResults
            };
        }

        #region SQLBuilder Members

        public void BuildExpression(StringBuilder builder)
        {
            if (selectExpressions.Count == 0)
            {
                throw new IncompleteQueryException();
            }

            BuildSelectClause(builder);

            BuildFromClause(builder);

            if (whereConstraint != null)
            {
                builder.Append(" WHERE ");

                whereConstraint.BuildExpression(builder);
            }

            BuildOrderByClause(builder);

            BuildLimitClause(builder);

            builder.Append(";");
        }

        public void AddSelectClause(string selectClause)
        {
            selectExpressions.Add(selectClause);
        }

        public void AddSelectTypeClause(string TableName, Type typeToSelect)
        {
            var annotatedProperties = from property in typeToSelect.GetProperties()
                                      where property.GetCustomAttributes(typeof(FieldMappingAttribute), false).Any()
                                      select property;

            foreach (PropertyInfo property in annotatedProperties)
            {
                FieldMappingAttribute attr = (FieldMappingAttribute) property.GetCustomAttributes(typeof(FieldMappingAttribute), false).Single();
                AddSelectClause(string.Format("{0}.{1} AS {2}", TableName, attr.FieldName, attr.UniqueFieldAlias));
            }
        }

        public void AddFromClause(string fromClause)
        {
            fromExpressions.Add(fromClause);
        }

        public void AddJoinClause(string joinKeyword, string tableToJoin, string condition)
        {
            joins.Add(new JoinDetails(joinKeyword, tableToJoin, condition));
        }

        public void AddWhereClause(string whereClause, ExpressionType combine)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                throw new ArgumentException("whereClause");
            }

            if ((combine != ExpressionType.And)
                && (combine != ExpressionType.Or))
            {
                throw new ArgumentOutOfRangeException("Combine operator must be AND or OR");
            }

            if (whereConstraint == null)
            {
                whereConstraint = new AtomicConstraint(whereClause);
            }
            else
            {
                whereConstraint = whereConstraint.CombineConstraint(new AtomicConstraint(whereClause), combine);
            }
        }

        public void AddOrderByClause(string orderBy, SortDirection direction)
        {
            orderExpressions.Add(new OrderExpression() { Expression = orderBy, Direction = direction });
        }

        public void AddCountClause()
        {
            countQuery = true;
        }

        public void SkipResults(int numResults)
        {
            skipResults = numResults;
        }

        public void TakeResults(int numResults)
        {
            takeResults = numResults;
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Private methods

        private void BuildSelectClause(StringBuilder builder)
        {
            builder.Append("SELECT ");

            if (countQuery)
            {
                builder.Append("COUNT(*) AS numrows");
            }
            else
            {
                builder.Append("DISTINCT ");

                bool first = true;

                foreach (string expression in selectExpressions)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(expression);
                    first = false;
                }
            }
        }

        private void BuildFromClause(StringBuilder builder)
        {
            if (fromExpressions.Count > 0)
            {
                builder.Append(" FROM ");

                bool first = true;

                foreach (string expression in fromExpressions)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(expression);
                    first = false;
                }

                // Note that we only add JOIN entries if we already have at least one FROM
                // entry. This makes sense, since you can't JOIN without a FROM, but perhaps
                // we ought to throw an error if this fails?
                foreach (JoinDetails join in joins)
                {
                    builder.Append(" ");
                    builder.Append(join.JoinKeyword);
                    builder.Append(" ");
                    builder.Append(join.TableName);
                    builder.Append(" ON ");
                    builder.Append(join.Constraint);
                }
            }
        }

        private void BuildOrderByClause(StringBuilder builder)
        {
            if (orderExpressions.Count > 0)
            {
                builder.Append(" ORDER BY ");

                bool first = true;

                foreach (OrderExpression expression in orderExpressions)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(expression.Expression);

                    if (expression.Direction == SortDirection.Descending)
                    {
                        builder.Append(" DESC");
                    }

                    first = false;
                }
            }
        }

        private void BuildLimitClause(StringBuilder builder)
        {
            if (skipResults.HasValue && takeResults.HasValue)
            {
                builder.AppendFormat(" LIMIT {0}, {1}", skipResults.Value, takeResults.Value);
            }
            else if (takeResults.HasValue)
            {
                builder.AppendFormat(" LIMIT {0}", takeResults.Value);
            }
            
            // TODO: The remaining case can't actually be implemented in MySQL. We can fudge it by setting
            // a skip value and a very large take value, but perhaps we should log a warning?
        }

        #endregion

        #region Private data members

        private struct OrderExpression
        {
            public string Expression;
            public SortDirection Direction;
        }

        private List<string> selectExpressions;
        private List<string> fromExpressions;
        private List<JoinDetails> joins;
        private Constraint whereConstraint;
        private List<OrderExpression> orderExpressions;
        private Nullable<int> skipResults;
        private Nullable<int> takeResults;

        /// <summary>
        /// Set this to true to make this statement select a count of matching rows rather than
        /// the rows themselves.
        /// </summary>
        private bool countQuery;

        #endregion
    }
}
