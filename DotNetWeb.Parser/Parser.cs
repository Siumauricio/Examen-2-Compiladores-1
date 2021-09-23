using DotNetWeb.Core;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using DotNetWeb.Core.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Type = DotNetWeb.Core.Type;

namespace DotNetWeb.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }
        public Statement Parse()
        {
           string path = "C:\\Users\\carlo\\Desktop\\DotNetWebTemplateCompiler-master\\DotNetWeb.Parser\\codigo.html";
           var stmt = Program();
            stmt.ValidateSemantic();
            stmt.Interpret();
            string code = stmt.Generate();
            Console.WriteLine(code);
            try {
                if (File.Exists(path)) {
                    File.Delete(path);
                }
                using (FileStream fs = File.Create(path)) {
                    Byte[] codex = new UTF8Encoding(true).GetBytes(code);
                    fs.Write(codex, 0, codex.Length);
                }
            } catch {
                throw new ApplicationException("Error Creating HTML file");
            }
            return stmt;
        }

        private Statement Program()
        {
           EnvironmentManager.PushContext();
           var block =  Init();
           var block2= Template();
            //string s = block2.Generate();
            return new SequenceStatement(block, block2);
        }

        private Statement Template()
        {
            return new SequenceStatement(Tag(), InnerTemplate());
        }
        
        private Statement InnerTemplate()
        {
            if (this.lookAhead.TokenType == TokenType.LessThan)
            {
                return Template();
            }
            return null;
        }
        private Statement Tag()
        {
            Match(TokenType.LessThan);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            var statement =  Stmts();
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            return statement;
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.OpenBrace)
            {
                return new SequenceStatement(Stmt(), Stmts());

            }
            return null;
        }

        private Statement Stmt()
        {
            Match(TokenType.OpenBrace);
            switch (this.lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    var expression = Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    return new PrintStatement(expression as TypedExpression);
                case TokenType.Percentage:
                   var stmt = IfStmt();
                    return stmt;
                case TokenType.Hyphen:
                    var fora = ForeachStatement();
                    return fora;
                default:
                    throw new ApplicationException("Unrecognized statement");
            }
        }

        private Statement ForeachStatement()
        {
            Match(TokenType.Hyphen);
            Match(TokenType.Percentage);
            Match(TokenType.ForEeachKeyword);
            var localvar = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.InKeyword);
            var varList = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            Id id = null;
            var result = EnvironmentManager.GetSymbolForEvaluation(varList.Lexeme);
            if (result == null) {
                throw new ApplicationException("Lista no existe!");
            }
            id = new Id(localvar, result.Id.GetExpressionType());
            EnvironmentManager.AddVariable(localvar.Lexeme, id);
            EnvironmentManager.UpdateVariable(localvar.Lexeme, result.Value);
            var statement = Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndForEachKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
           
            return new ForEachStatement(localvar, varList, statement);
        }
        
        private Statement IfStmt()
        {
            Match(TokenType.Percentage);
            Match(TokenType.IfKeyword);
           var expression =  Eq();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var stmt = Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndIfKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return new IfStatement(expression as TypedExpression, stmt);

        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }
            return expression;

        }

        private Expression Rel()
        {
             var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;

        }
    

        private Expression Expr()
        {
            var expression = Term();

            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Hyphen)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;

        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Slash)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;

        }
        List<dynamic> listConfig = new List<dynamic>();
        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Eq();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.OpenBracket:
                    Match(TokenType.OpenBracket);
                    
                    constant = new Constant(lookAhead, symbol2.Id.GetExpressionType());
                    List<dynamic> list = new List<dynamic>();
                    ExprList(list);
                    isList = true;
                    listConfig = list;
                    Match(TokenType.CloseBracket);
                    symbol2 = null;
                    return constant;
                default:
                    var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
        }
        bool isList = false;
        private void ExprList(List<dynamic> list)
        {
            var items = Eq();
            list.Add(items as TypedExpression);
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return;
            }
            Match(TokenType.Comma);
            ExprList(list);
        }

        private Statement Init()
        {
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.InitKeyword);
            var code = Code();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return code;
        }

        private Statement Code()
        {
           Decls();
           var assignation =  Assignations();
           return assignation;
        }

        private Statement Assignations()
        {
            if (this.lookAhead.TokenType == TokenType.Identifier)
            {
                return new SequenceStatement(Assignation(), Assignations());
            }
            return null;
        }
        Symbol symbol2 = null;
        private Statement Assignation()
        {
           Symbol symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
            Match(TokenType.Identifier);
            symbol2 = symbol;
            Match(TokenType.Assignation);
            var expression = Eq();
            Match(TokenType.SemiColon);
            if (isList) {
                isList = false;
                EnvironmentManager.UpdateVariable(symbol.Id.Token.Lexeme, listConfig);
                return new AssignationStatement(symbol.Id, expression as TypedExpression, listConfig) ;
            }
            return new AssignationStatement(symbol.Id, expression as TypedExpression);
        }

        private void Decls()
        {
            Decl();
            InnerDecls();
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.FloatListKeyword:
                    Match(TokenType.FloatListKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.FloatList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.IntList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringListKeyword:
                    Match(TokenType.StringListKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.StringList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                default:
                    throw new ApplicationException($"Unsupported type {this.lookAhead.Lexeme}");
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }

        private bool LookAheadIsType()
        {
            return this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.IntListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatListKeyword ||
                this.lookAhead.TokenType == TokenType.StringListKeyword;

        }
    }
}
