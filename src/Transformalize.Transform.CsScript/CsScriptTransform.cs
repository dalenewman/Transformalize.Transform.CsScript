using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using csscript;
using CSScriptLibrary;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Transforms.CsScript {
    public class CsScriptTransform : BaseTransform {

        private static readonly Dictionary<string, ICsScriptTransform> LocalCache = new Dictionary<string, ICsScriptTransform>();
        private static readonly Dictionary<string, MethodDelegate<object>> RemoteCache = new Dictionary<string, MethodDelegate<object>>();

        private readonly ICsScriptTransform _local;
        private readonly MethodDelegate<object> _remote;

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
            var cb = new StringBuilder();

            var methodName = Utility.GetMethodName(Context);
            if (char.IsLower(methodName[0])) {
                methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);
            }

            if (!Context.Field.Remote) {
                cb.AppendLine("using Transformalize.Transforms.CsScript;");
                cb.AppendLine("using System;");
                cb.AppendLine();
                cb.AppendLine("public class " + methodName + " : ICsScriptTransform {");
                cb.AppendLine();
            }

            cb.AppendLine("  public object Transform(object[] row){");

            foreach (var field in fields) {
                var type = ToSystemTypeName(field.Type);
                var name = Protection.KeyWords.Contains(Utility.Identifier(field.Alias)) ? "@" + Utility.Identifier(field.Alias) : Utility.Identifier(field.Alias);
                var index = Context.Entity.IsMaster ? field.MasterIndex : field.Index;
                cb.AppendLine($"    {type} {name} = ({type}) row[{index}];");
            }

            cb.AppendLine();
            cb.AppendLine("    " + Context.Operation.Script);
            cb.AppendLine("  }");
            if (!Context.Field.Remote) {
                cb.AppendLine("}");
            }

            var code = cb.ToString();

            if (LocalCache.ContainsKey(code) && LocalCache[code] != null) {
                _local = LocalCache[code];
                Context.Warn("Using cached local code");
                return;
            }

            if (RemoteCache.ContainsKey(code) && RemoteCache[code] != null) {
                _remote = RemoteCache[code];
                Context.Warn("Using cached remote code");
                return;
            }

            try {
                if (Context.Field.Remote) {
                    _remote = CSScript.Evaluator.CreateDelegateRemotely<object>(code);
                    RemoteCache[code] = _remote;
                } else {
                    _local = CSScript.Evaluator.LoadCode<ICsScriptTransform>(code);
                    LocalCache[code] = _local;
                }

            } catch (CompilerException e) {
                Run = false;
                Context.Error(e.Message);
                Context.Error(code.Replace("{", "{{").Replace("}", "}}"));
            }

        }

        public override IRow Operate(IRow row) {
            throw new NotImplementedException("This is not called because Operate over rows is implemented!");
        }

        public override IEnumerable<IRow> Operate(IEnumerable<IRow> rows) {
            if (Context.Field.Remote) {
                foreach (var row in rows) {
                    row[Context.Field] = _remote(new object[] { row.ToArray() });
                    yield return row;
                }
            } else {
                foreach (var row in rows) {
                    row[Context.Field] = _local.Transform(row.ToArray());
                    yield return row;
                }
            }
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


        public static string AssemblyDirectory {
            get {
                var codeBase = typeof(Process).Assembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

    }


}
