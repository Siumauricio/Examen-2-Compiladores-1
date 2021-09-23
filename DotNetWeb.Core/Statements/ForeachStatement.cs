using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements {
    public class ForEachStatement : Statement {
        public ForEachStatement(Token token1, Token token2, Statement statement) {
            Token1 = token1;
            Token2 = token2;
            Statement = statement;
        }
        public Statement Statement { get; }
        public Token Token1 { get; }
        public Token Token2 { get; }

        public override void ValidateSemantic() {
            var result = EnvironmentManager.GetSymbolForEvaluation(Token2.Lexeme);
            EnvironmentManager.UpdateVariable(Token1.Lexeme, result.Value);
        }

        public override void Interpret() {
            //EnvironmentManager.UpdateVariable(Token1.Lexeme, Expression.Evaluate());
        }

        public override string Generate() {
            string value =Statement.Generate();
            return value;
        }
    }
}
