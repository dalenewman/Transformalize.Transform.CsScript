using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Transforms.CsScript {
    public abstract class CodeCommon : BaseTransform {
        protected CodeCommon(IContext context = null) : base(context, "object") { }

        public override IRow Operate(IRow row) {
            throw new NotImplementedException("This is not called because Operate over rows is implemented!");
        }

        protected string NormalizeScript(string script) {
            if (!script.Contains("return ")) {
                script = "return " + script;
            }

            if (!script.EndsWith(";")) {
                script += ";";
            }

            return script;
        }

        protected void AppendCommonCode(StringBuilder cb) {
            cb.AppendLine("  public object Transform(object[] row){");

            foreach (var field in Context.Entity.GetFieldMatches(Context.Operation.Script)) {
                var type = ToSystemTypeName(field.Type);
                var name = Protection.KeyWords.Contains(Utility.Identifier(field.Alias)) ? "@" + Utility.Identifier(field.Alias) : Utility.Identifier(field.Alias);
                var index = Context.Entity.IsMaster ? field.MasterIndex : field.Index;
                cb.AppendLine($"    {type} {name} = ({type}) row[{index}];");
            }

            cb.AppendLine();
            cb.AppendLine("    " + Context.Operation.Script);
            cb.AppendLine("  }");
        }

        protected static string ToSystemTypeName(string typeIn) {
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

        protected static string AssemblyDirectory {
            get {
                var codeBase = typeof(Process).Assembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

    }


}
