using System.Collections.Generic;
using CSScriptLibrary;
using Transformalize.Actions;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms.CsScript {
    public class RemoteUnload : IAction {

        private readonly ActionResponse _response;
        public RemoteUnload(Action action) {
            _response = new ActionResponse(200, "Ok") { Action = action };
        }

        public ActionResponse Execute() {
            var keys = new List<string>(CodeRemote.RemoteCache.Keys);
            foreach (var key in keys) {
                if (CodeRemote.RemoteCache.ContainsKey(key)) {
                    CodeRemote.RemoteCache[key] = null;
                }
            }

            var domain = CSScript.Evaluator.GetRemoteDomain();
            domain?.Unload();
            return _response;
        }
    }
}