using System;
using System.Collections.Generic;
using System.Text;
using CSScriptLibrary;
using Transformalize.Contracts;

namespace Transformalize.Transforms.CsScript {

    public interface ICode {
        object Transform(object[] rowData);
    }

    public class CodeLocal : CodeCommon {

        private readonly ICode _local;
        private static readonly Dictionary<string, ICode> LocalCache = new Dictionary<string, ICode>();
        
        public CodeLocal(IContext context = null) : base(context) {
            if (IsMissingContext()) {
                return;
            }

            if (IsMissing(Context.Operation.Script)) {
                Error($"You are missing a script for a cs transform in {Context.Field.Alias}.");
                return;
            }

            Context.Operation.Script = NormalizeScript(Context.Operation.Script);

            var cb = new StringBuilder();

            var methodName = Utility.GetMethodName(Context);
            if (char.IsLower(methodName[0])) {
                methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);
            }

            cb.AppendLine("using Transformalize.Transforms.CsScript;");
            cb.AppendLine("using System;");
            cb.AppendLine();
            cb.AppendLine("public class " + methodName + " : ICode {");
            cb.AppendLine();

            AppendCommonCode(cb);

            cb.AppendLine("}");

            var code = cb.ToString();

            if (LocalCache.ContainsKey(code) && LocalCache[code] != null) {
                _local = LocalCache[code];
                Context.Warn("Using cached local code");
                return;
            }

            try {
                Context.Warn("Compiling and caching local code");
                _local = CSScript.Evaluator.LoadCode<ICode>(code);
                LocalCache[code] = _local;
            } catch (Exception e) {
                Run = false;
                Context.Error(e.Message);
                Context.Error(code.Replace("{", "{{").Replace("}", "}}"));
            }
        }

        public override IEnumerable<IRow> Operate(IEnumerable<IRow> rows) {
            foreach (var row in rows) {
                row[Context.Field] = _local.Transform(row.ToArray());
                yield return row;
            }
        }
    }
}