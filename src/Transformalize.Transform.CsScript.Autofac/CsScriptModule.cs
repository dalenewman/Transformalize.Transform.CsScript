﻿using System;
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

            var signatures = new CsScriptTransform().GetSignatures().ToArray();

            if (!_setup) {
                CSScript.EvaluatorConfig.Engine = EvaluatorEngine.CodeDom;
                CSScript.EvaluatorConfig.RefernceDomainAsemblies = true;
                CSScript.GlobalSettings.SearchDirs = AssemblyDirectory + ";" + Path.Combine(AssemblyDirectory, "plugins");
                CSScript.AssemblyResolvingEnabled = true;
                CSScript.CacheEnabled = true;
                CSSEnvironment.SetScriptTempDir(Path.Combine(AssemblyDirectory, "plugins", "cs-script"));

                _setup = true;
            }

            // get methods and shorthand from builder
            _methods = builder.Properties.ContainsKey("Methods") ? (HashSet<string>)builder.Properties["Methods"] : new HashSet<string>();
            _shortHand = builder.Properties.ContainsKey("ShortHand") ? (ShorthandRoot)builder.Properties["ShortHand"] : new ShorthandRoot();
            RegisterShortHand(signatures);
            RegisterTransform(builder, c => new CsScriptTransform(c), signatures);
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

        private void RegisterTransform(ContainerBuilder builder, Func<IContext, ITransform> getTransform, IEnumerable<OperationSignature> signatures) {
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
