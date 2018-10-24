using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using csscript;
using Cfg.Net.Shorthand;
using CSScriptLibrary;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Parameter = Cfg.Net.Shorthand.Parameter;

namespace Transformalize.Transforms.CsScript.Autofac {
    public class CsScriptModule : Module {
        private static bool _setup;

        private HashSet<string> _methods;
        private ShorthandRoot _shortHand;

        protected override void Load(ContainerBuilder builder) {

            var signatures = new CodeLocal().GetSignatures().ToArray();

            if (!_setup) {
                AppDomain.MonitoringIsEnabled = true;
                CSScript.EvaluatorConfig.Access = EvaluatorAccess.Singleton;
                CSScript.EvaluatorConfig.Engine = EvaluatorEngine.CodeDom;
                CSScript.GlobalSettings.SearchDirs = AssemblyDirectory + ";" + Path.Combine(AssemblyDirectory, "plugins");
                CSScript.CacheEnabled = true;
                CSSEnvironment.SetScriptTempDir(Path.Combine(AssemblyDirectory, "plugins", "cs-script"));
                _setup = true;
            }

            // get methods and shorthand from builder
            _methods = builder.Properties.ContainsKey("Methods") ? (HashSet<string>)builder.Properties["Methods"] : new HashSet<string>();
            _shortHand = builder.Properties.ContainsKey("ShortHand") ? (ShorthandRoot)builder.Properties["ShortHand"] : new ShorthandRoot();

            RegisterShortHand(signatures);
            RegisterTransform(builder, c => c.Field.Remote ? (ITransform)new CodeRemote(c) : new CodeLocal(c), signatures);

            if (!builder.Properties.ContainsKey("Process"))
                return;

            var process = (Process)builder.Properties["Process"];
            if (process == null)
                return;
            
            if (process.Entities.Any() && process.GetAllTransforms().Any(t => t.Method == "cs" || t.Method == "csharp")) {
                var action = new Configuration.Action { Type = "cs-script", Before = false, After = true, Key = "cs-script" };
                builder.Register<IAction>((c) => new RemoteUnload(c.Resolve<IContext>(), action)).Named<IAction>("cs-script");
                process.Actions.Add(action);
            }


        }
        private void RegisterShortHand(IEnumerable<OperationSignature> signatures) {

            foreach (var s in signatures) {
                if (!_methods.Add(s.Method)) {
                    continue;
                }

                var method = new Method { Name = s.Method, Signature = s.Method, Ignore = s.Ignore };
                _shortHand.Methods.Add(method);

                var signature = new Signature {
                    Name = s.Method,
                    NamedParameterIndicator = s.NamedParameterIndicator
                };

                foreach (var parameter in s.Parameters) {
                    signature.Parameters.Add(new Parameter {
                        Name = parameter.Name,
                        Value = parameter.Value
                    });
                }
                _shortHand.Signatures.Add(signature);
            }
        }

        private static void RegisterTransform(ContainerBuilder builder, Func<IContext, ITransform> getTransform, IEnumerable<OperationSignature> signatures) {
            foreach (var s in signatures) {
                builder.Register((c, p) => getTransform(p.Positional<IContext>(0))).Named<ITransform>(s.Method);
            }
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
