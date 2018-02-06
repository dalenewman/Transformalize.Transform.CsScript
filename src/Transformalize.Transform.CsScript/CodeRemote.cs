using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CSScriptLibrary;
using Transformalize.Contracts;

namespace Transformalize.Transforms.CsScript {
    public class CodeRemote : CodeCommon {

        private readonly MethodDelegate<object> _remote;
        public static readonly Dictionary<string, MethodDelegate<object>> RemoteCache = new Dictionary<string, MethodDelegate<object>>();

        public CodeRemote(IContext context = null) : base(context) {
            if (IsMissingContext()) {
                return;
            }

            if (IsMissing(Context.Operation.Script)) {
                Error($"You are missing a script for a cs transform in {Context.Field.Alias}.");
                return;
            }

            Context.Operation.Script = NormalizeScript(Context.Operation.Script);

            var cb = new StringBuilder();
            AppendCommonCode(cb);
            var code = cb.ToString();

            if (RemoteCache.ContainsKey(code) && RemoteCache[code] != null) {
                _remote = RemoteCache[code];
                Context.Warn("Using cached remote code");
                return;
            }

            try {
                Context.Warn("Compiling and caching remote code");
                _remote = CSScript.Evaluator.CreateDelegateRemotely<object>(code);
                
                RemoteCache[code] = _remote;

            } catch (Exception e) {
                Run = false;
                Context.Error(e.Message);
                Context.Error(code.Replace("{", "{{").Replace("}", "}}"));
            }

        }

        public override IEnumerable<IRow> Operate(IEnumerable<IRow> rows) {
            foreach (var row in rows) {
                row[Context.Field] = _remote(new object[] { row.ToArray() });
                yield return row;
            }
        }
    }
}