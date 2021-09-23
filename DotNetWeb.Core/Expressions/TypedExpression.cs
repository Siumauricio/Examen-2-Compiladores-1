using DotNetWeb.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Expressions {
    public abstract class TypedExpression : Expression, IExpressionEvaluate {
        public TypedExpression(Token token, Type type)
            : base(token, type) {
        }

        public abstract dynamic Evaluate();

        public abstract Type GetExpressionType();
    }
}
