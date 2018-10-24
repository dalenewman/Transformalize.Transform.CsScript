### Overview

This adds C# transform to Transformalize using [CS-Script](https://github.com/oleg-shilo/cs-script).  It is a plug-in compatible with Transformalize 0.3.6-beta.

Build the Autofac project and put it's output into Transformalize's *plugins* folder.

### Usage

```xml
<cfg name="Test">
    <entities>
        <add name="Test">
            <rows>
                <add text="SomethingWonderful" number="2" />
            </rows>
            <fields>
                <add name="text" />
                <add name="number" type="int" />
            </fields>
            <calculated-fields>
                <add name="csharped" t='csscript(return text + " " + number;)' />
            </calculated-fields>
        </add>
    </entities>
</cfg>
```

This produces `SomethingWonderful 2`

Note: This csharp transform allows you to set a `remote` attribute on the field to `true`.  Doing 
this runs the code in a remote `AppDomain`.  This avoids the memory leak associated with running 
dyanamically loaded c# assemblies in the host's `AppDomain`.  Unfortunately, running the code 
remotely is only half as fast as running it in-process.

### Benchmark

``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.16299.251 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742183 Hz, Resolution=364.6730 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0  [AttachedDebugger]
  LegacyJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2633.0;compatjit-v4.7.2633.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|                  Method |    Mean |    Error |   StdDev | Scaled | ScaledSD |
|------------------------ |--------:|---------:|---------:|-------:|---------:|
|       &#39;10000 test rows&#39; | 1.052 s | 0.0137 s | 0.0128 s |   1.00 |     0.00 |
|  &#39;10000 1 local csharp&#39; | 1.065 s | 0.0139 s | 0.0130 s |   1.01 |     0.02 |
| &#39;10000 1 remote csharp&#39; | 1.660 s | 0.0142 s | 0.0132 s |   1.58 |     0.02 |
