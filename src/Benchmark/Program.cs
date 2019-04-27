using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Transformalize.Contracts;
using Transformalize.Containers.Autofac;
using Transformalize.Logging;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Transforms.CsScript.Autofac;
using Transformalize.Configuration;

namespace Benchmark {

   [LegacyJitX64Job]
   public class Benchmarks {

      [Benchmark(Baseline = true, Description = "10000 test rows")]
      public void TestRows() {
         var logger = new NullLogger();
         using (var outer = new ConfigurationContainer(new CsScriptModule()).CreateScope(@"files\bogus.xml?Size=10000", logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new Container(new CsScriptModule(), new BogusModule()).CreateScope(process, logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "10000 1 local csharp")]
      public void CSharpRows() {
         var logger = new NullLogger();
         using (var outer = new ConfigurationContainer(new CsScriptModule()).CreateScope(@"files\bogus-csharp.xml?Size=10000", logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new Container(new CsScriptModule(), new BogusModule()).CreateScope(process, logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "10000 1 remote csharp")]
      public void CSharpRemoteRows() {
         var logger = new NullLogger();
         using (var outer = new ConfigurationContainer(new CsScriptModule()).CreateScope(@"files\bogus-csharp.xml?Size=10000&Remote=true", logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new Container(new CsScriptModule(), new BogusModule()).CreateScope(process, new NullLogger())) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }
   }

   public class Program {
      private static void Main(string[] args) {
         var summary = BenchmarkRunner.Run<Benchmarks>();
         Console.WriteLine(summary);
      }
   }
}
