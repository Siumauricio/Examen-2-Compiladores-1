using DotNetWeb.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements {
    public abstract class Statement : Node, ISemanticValidation, IStatementEvaluate {
        public abstract void Interpret();

        public abstract void ValidateSemantic();

        public abstract string Generate();

        public virtual string GetCodeInit() {
            var code = string.Empty;
            return code;
        }
    }
}
