using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Alba.Routing
{
    public class MethodCallParser : ExpressionVisitor
    {
        internal readonly List<object> Arguments = new List<object>();

        public static List<object> ToArguments(Expression expression)
        {
            var parser = new MethodCallParser();
            parser.Visit(expression);

            return parser.Arguments;
        }

        public static List<object> ToArguments<T>(Expression<Action<T>> expression)
        {
            var parser = new MethodCallParser();
            parser.Visit(expression);

            return parser.Arguments;
        } 

        private MethodCallParser()
        {
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Arguments.Add(node.Value);

            return base.VisitConstant(node);
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            if (member.Expression is ConstantExpression &&
                member.Member is FieldInfo)
            {
                object container =
                    ((ConstantExpression)member.Expression).Value;
                object value = ((FieldInfo)member.Member).GetValue(container);

                Arguments.Add(value);

                return member;
            }

            return base.VisitMember(member);
        }
    }
}