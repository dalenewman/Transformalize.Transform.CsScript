using System;
using System.Collections.Generic;
using System.Linq;
using CSScriptLibrary;
using Transformalize.Contracts;
using Action = Transformalize.Configuration.Action;

namespace Transformalize.Transforms.CsScript {
    public class RemoteUnload : IAction {
        private readonly IContext _context;

        private readonly ActionResponse _response;
        public RemoteUnload(IContext context, Action action)
        {
            _context = context;
            _response = new ActionResponse(200, "Ok") { Action = action };
        }

        public ActionResponse Execute() {
            var keys = new List<string>(CodeRemote.RemoteCache.Keys);
            //foreach (var key in keys) {
            //    if (CodeRemote.RemoteCache.ContainsKey(key)) {
            //        CodeRemote.RemoteCache[key] = null;
            //    }
            //}

            if (keys.Any()) {
                var domain = CSScript.Evaluator.GetRemoteDomain();
                if (domain == null) {
                    _response.Code = 404;
                    _response.Message = "The remote appdomain is not found";
                } else {
                    //domain.Unload();

                    //_response.Message = "Unloaded the remote appdomain.";
                    //_response.Code = 500;
                    //_response.Message = $"Local Memory Size: {AppDomain.CurrentDomain.MonitoringSurvivedMemorySize},  Remote Memory Size: {domain.MonitoringSurvivedMemorySize}";
                    _context.Warn($"Local Memory: {AppDomain.CurrentDomain.MonitoringSurvivedMemorySize}");
                    _context.Warn($"Remote Memory: {domain.MonitoringSurvivedMemorySize}");
                }
            }

            return _response;
        }
    }
}