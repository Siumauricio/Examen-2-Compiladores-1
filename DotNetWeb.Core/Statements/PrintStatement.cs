using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements {
   public class PrintStatement : Statement {
        public PrintStatement(TypedExpression expression) {
            Expression = expression;

        }
        public TypedExpression Expression { get; }

        public override string Generate() {
            var code = GetCodeInit();
            TokenType token = 0;
            if (Expression.GetExpressionType() == Type.StringList) {
                token = TokenType.StringConstant;
            }
            if (Expression.GetExpressionType() == Type.FloatList) {
                token = TokenType.FloatConstant;
            }
            if (Expression.GetExpressionType() == Type.IntList) {
                token = TokenType.IntConstant;
            }
            if (Expression.GetExpressionType() == Type.StringList || Expression.GetExpressionType() == Type.FloatList || Expression.GetExpressionType() == Type.IntList) {
                var Result = EnvironmentManager.GetSymbolForEvaluation(Expression.Token.Lexeme);
                for (int i = 0; i < Result.Value.Count; i++) {
                    if (token !=  Result.Value[i].Token.TokenType) {
                        throw new ApplicationException($"Se encontro un tipo de dato no valido en la lista!");
                    } 
                    code += $"<div> {Result.Value[i].Token.Lexeme} </div>"+Environment.NewLine;
                }
            } else {
                code += $"<div> {Expression.Evaluate()} </div>" + Environment.NewLine;
            }
            return code;
        }

        public override void Interpret() {
            //EnvironmentManager.GetSymbolForEvaluation(Expression.Token.Lexeme);
        }

        public override void ValidateSemantic() {
     
        }
    }
}
