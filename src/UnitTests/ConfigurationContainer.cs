#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using Autofac;
using Cfg.Net.Shorthand;
using Transformalize.Configuration;
using Transformalize.Transforms.CsScript.Autofac;

namespace UnitTests {
    public class ConfigurationContainer {

        private readonly HashSet<string> _methods = new HashSet<string>();
        private readonly ShorthandRoot _shortHand = new ShorthandRoot();
        public ILifetimeScope CreateScope(string cfg) {

            var builder = new ContainerBuilder();
            builder.Properties["ShortHand"] = _shortHand;
            builder.Properties["Methods"] = _methods;

            builder.RegisterModule(new CsScriptModule());
            builder.Register((c, p) => _shortHand).As<ShorthandRoot>().InstancePerLifetimeScope();

            builder.Register(ctx=> new Process(cfg, new ShorthandCustomizer(ctx.Resolve<ShorthandRoot>(), new[] {"fields", "calculated-fields"}, "t", "transforms", "method"))).As<Process>().InstancePerDependency();  // because it has state, if you run it again, it's not so good
            return builder.Build().BeginLifetimeScope();
        }
    }

}
