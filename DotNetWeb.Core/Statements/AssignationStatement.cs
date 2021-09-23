using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements {
    public class AssignationStatement : Statement {
        public AssignationStatement(Id id, TypedExpression expression) {
            Id = id;
            Expression = expression;
        }
        public AssignationStatement(Id id, TypedExpression expression,dynamic _value) {
            Id = id;
            Expression = expression;
            values = _value;
        }

        public Id Id { get; }
        public dynamic values { get; }
        public TypedExpression Expression { get; }

        public override string Generate() {
            var code = GetCodeInit();
            if (Id.GetExpressionType() == Type.StringList || Id.GetExpressionType() == Type.FloatList || Id.GetExpressionType() == Type.IntList ) {
                code += $"<div>{Id.Generate()} = [";
                for (int i = 0; i < values.Count; i++) {
                    code += $"{values[i].Token.Lexeme}";
                    if (i+1!=(values.Count)) {
                        code += ",";
                    }

                }
              
                code += "] </div> "+Environment.NewLine;
            } else {
                code += $"<div>{Id.Generate()} = {Expression.Generate()}</div>"+Environment.NewLine;
            }
            return code;
        }

        public override void Interpret() {
            EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
        }

        public override void ValidateSemantic() {
            if (Id.GetExpressionType() != Expression.GetExpressionType()) {
                throw new ApplicationException($"Type {Id.GetExpressionType()} is not assignable to {Expression.GetExpressionType()}");
            }
        }
    }
}