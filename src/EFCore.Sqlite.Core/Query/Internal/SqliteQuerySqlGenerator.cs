// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Query.SqlExpressions.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQuerySqlGenerator : QuerySqlGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            GlobExpression globExpression => VisitGlob(globExpression),
            RegexpExpression regexpExpression => VisitRegexp(regexpExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GetOperator(SqlBinaryExpression binaryExpression)
        => binaryExpression.OperatorType == ExpressionType.Add
            && binaryExpression.Type == typeof(string)
                ? " || "
                : base.GetOperator(binaryExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        if (selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            Sql.AppendLine()
                .Append("LIMIT ");

            Visit(
                selectExpression.Limit
                ?? new SqlConstantExpression(Expression.Constant(-1), selectExpression.Offset!.TypeMapping));

            if (selectExpression.Offset != null)
            {
                Sql.Append(" OFFSET ");

                Visit(selectExpression.Offset);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
        // Sqlite doesn't support parentheses around set operation operands
        => Visit(operand);

    private Expression VisitGlob(GlobExpression globExpression)
    {
        Visit(globExpression.Match);

        if (globExpression.IsNegated)
        {
            Sql.Append(" NOT");
        }

        Sql.Append(" GLOB ");
        Visit(globExpression.Pattern);

        return globExpression;
    }

    private Expression VisitRegexp(RegexpExpression regexpExpression)
    {
        Visit(regexpExpression.Match);

        if (regexpExpression.IsNegated)
        {
            Sql.Append(" NOT");
        }

        Sql.Append(" REGEXP ");
        Visit(regexpExpression.Pattern);

        return regexpExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        if (jsonScalarExpression.Path.Count == 1
            && jsonScalarExpression.Path[0].ToString() == "$")
        {
            Visit(jsonScalarExpression.JsonColumn);

            return jsonScalarExpression;
        }

        Sql.Append("json_extract(");

        Visit(jsonScalarExpression.JsonColumn);

        Sql.Append(", '");
        foreach (var pathSegment in jsonScalarExpression.Path)
        {
            if (pathSegment.PropertyName != null)
            {
                Sql.Append((pathSegment.PropertyName == "$" ? "" : ".") + pathSegment.PropertyName);
            }

            if (pathSegment.ArrayIndex != null)
            {
                Sql.Append("[");

                if (pathSegment.ArrayIndex is SqlConstantExpression)
                {
                    Visit(pathSegment.ArrayIndex);
                }
                else
                {
                    Sql.Append("' || ");
                    if (pathSegment.ArrayIndex is SqlParameterExpression)
                    {
                        Visit(pathSegment.ArrayIndex);
                    }
                    else
                    {
                        Sql.Append("(");
                        Visit(pathSegment.ArrayIndex);
                        Sql.Append(")");
                    }

                    Sql.Append(" || '");
                }

                Sql.Append("]");
            }
        }

        Sql.Append("')");

        return jsonScalarExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TryGetOperatorInfo(SqlExpression expression, out int precedence, out bool isAssociative)
    {
        // See https://sqlite.org/lang_expr.html#operators_and_parse_affecting_attributes
        (precedence, isAssociative) = expression switch
        {
            SqlBinaryExpression sqlBinaryExpression => sqlBinaryExpression.OperatorType switch
            {
                ExpressionType.Multiply => (900, true),
                ExpressionType.Divide => (900, false),
                ExpressionType.Modulo => (900, false),
                ExpressionType.Add when sqlBinaryExpression.Type == typeof(string) => (1100, true),
                ExpressionType.Add when sqlBinaryExpression.Type != typeof(string) => (800, true),
                ExpressionType.Subtract => (800, false),
                ExpressionType.And => (600, true),
                ExpressionType.Or => (600, true),
                ExpressionType.LessThan => (500, false),
                ExpressionType.LessThanOrEqual => (500, false),
                ExpressionType.GreaterThan => (500, false),
                ExpressionType.GreaterThanOrEqual => (500, false),
                ExpressionType.Equal => (500, false),
                ExpressionType.NotEqual => (500, false),
                ExpressionType.AndAlso => (200, true),
                ExpressionType.OrElse => (100, true),

                _ => default,
            },

            SqlUnaryExpression sqlUnaryExpression => sqlUnaryExpression.OperatorType switch
            {
                ExpressionType.Convert => (1300, false),
                ExpressionType.Not when sqlUnaryExpression.Type != typeof(bool) => (1200, false),
                ExpressionType.Negate => (1200, false),
                ExpressionType.Equal => (500, false), // IS NULL
                ExpressionType.NotEqual => (500, false), // IS NOT NULL
                ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool) => (300, false),

                _ => default,
            },

            CollateExpression => (1100, false),
            LikeExpression => (500, false),

            _ => default,
        };

        return precedence != default;
    }
}
