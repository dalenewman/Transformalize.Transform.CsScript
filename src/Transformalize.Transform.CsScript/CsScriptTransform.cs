using System.Collections.Generic;
using System.Text;
using csscript;
using CSScriptLibrary;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Transforms.CsScript {

    public interface ICsScriptTransform {
        object Transform(object[] rowData);
    }

    public class CsScriptTransform : BaseTransform {
        /*
         * Note: Caching (as well as debugging) is only compatible with CodeDOM
         * compiler engine: calls CSSCript.Load*, CSScript.Compile.* and CSScript.CodeDomEvaluator.*).
         * Thus caching will be effectively disabled for both CSScript.MonoEvaluator.* and CSScript.RoslynEvaluator.*.
         */
        private readonly ICsScriptTransform _transform;
        public CsScriptTransform(IContext context = null) : base(context, "object") {
            if (IsMissingContext()) {
                return;
            }

            if (IsMissing(Context.Operation.Script)) {
                Error($"You are missing a script for a cs transform in {Context.Field.Alias}.");
                return;
            }

            // handle csharp body or an expression
            if (!Context.Operation.Script.Contains("return ")) {
                Context.Operation.Script = "return " + Context.Operation.Script;
            }

            if (!Context.Operation.Script.EndsWith(";")) {
                Context.Operation.Script += ";";
            }

            var fields = Context.Entity.GetFieldMatches(Context.Operation.Script);
            var code = new StringBuilder();

            code.AppendLine("using Transformalize.Transforms.CsScript;");
            code.AppendLine();
            code.AppendLine("public class " + Utility.GetMethodName(Context) + " : ICsScriptTransform {");
            code.AppendLine();
            code.AppendLine("  public object Transform(object[] row){");

            foreach (var field in fields) {
                var type = ToSystemTypeName(field.Type);
                var name = Protection.KeyWords.Contains(Utility.Identifier(field.Alias)) ? "@" + Utility.Identifier(field.Alias) : Utility.Identifier(field.Alias);
                var index = Context.Entity.IsMaster ? field.MasterIndex : field.Index;
                code.AppendLine($"    {type} {name} = ({type}) row[{index}];");
            }

            code.AppendLine();
            code.AppendLine("    " +Context.Operation.Script);
            code.AppendLine("  }");
            code.AppendLine("}");
            var expanded = code.ToString();

            try {
                CSScript.AssemblyResolvingEnabled = true;
                _transform = CSScript.Evaluator.LoadCode<ICsScriptTransform>(expanded);
            } catch (CompilerException e) {
                Run = false;
                Context.Error(e.Message);
                Context.Error(expanded.Replace("{", "{{").Replace("}", "}}"));
                return;
            }

            Context.Debug(() => $"Code for {Context.Field.Alias} is:/r/n {expanded.Replace("{", "{{").Replace("}", "}}")}");
        }

        public override IRow Operate(IRow row) {
            row[Context.Field] = _transform.Transform(row.ToArray());
            return row;
        }

        public static string ToSystemTypeName(string typeIn) {
            string typeOut;
            switch (typeIn) {
                case "date":
                case "datetime":
                    typeOut = "DateTime";
                    break;
                case "single":
                case "int16":
                case "int32":
                case "int64":
                    typeOut = typeIn.Left(1).ToUpper() + typeIn.Substring(1);
                    break;
                default:
                    typeOut = typeIn;
                    break;
            }
            return typeOut;
        }

        public override IEnumerable<OperationSignature> GetSignatures() {
            return new[] {
                new OperationSignature("cs") {
                    Parameters = new List<OperationParameter> { new OperationParameter("script") }
                },
                new OperationSignature("csharp") {
                    Parameters = new List<OperationParameter> { new OperationParameter("script") }
                }
            };
        }

    }


}
