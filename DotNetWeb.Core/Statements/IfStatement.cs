using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements {
    public class IfStatement : Statement {
        public IfStatement(TypedExpression expression, Statement statement) {
            Expression = expression;
            Statement = statement;
        }

        public TypedExpression Expression { get; }
        public Statement Statement { get; }

        public override string Generate() {
            var code = GetCodeInit();
            if (Expression.Evaluate()) {
                code += $"{Statement.Generate()}{Environment.NewLine}";
            }
            return code;
        }

        public override void Interpret() {
            if (Expression.Evaluate()) {
                Statement.Interpret();
            }
        }

        public override void ValidateSemantic() {
            if (Expression.GetExpressionType() != Type.Bool) {
                throw new ApplicationException("A boolean is required in ifs");
            }
        }
    }
}