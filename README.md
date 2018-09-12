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
Frequency=2742188 Hz, Resolution=364.6723 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0  [AttachedDebugger]
  LegacyJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2633.0;compatjit-v4.7.2633.0
  LegacyJitX86 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0

Jit=LegacyJit  Runtime=Clr  

```
|                 Method |          Job | Platform |     Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------------------- |------------- |--------- |---------:|----------:|----------:|-------:|---------:|
|       &#39;5000 test rows&#39; | LegacyJitX64 |      X64 | 522.6 ms |  9.774 ms |  9.142 ms |   1.00 |     0.00 |
|  &#39;5000 1 local csharp&#39; | LegacyJitX64 |      X64 | 535.3 ms | 10.448 ms | 18.840 ms |   1.02 |     0.04 |
| &#39;5000 1 remote csharp&#39; | LegacyJitX64 |      X64 | 828.1 ms | 16.174 ms | 23.707 ms |   1.58 |     0.05 |
|                        |              |          |          |           |           |        |          |
|       &#39;5000 test rows&#39; | LegacyJitX86 |      X86 | 547.1 ms |  6.538 ms |  5.795 ms |   1.00 |     0.00 |
|  &#39;5000 1 local csharp&#39; | LegacyJitX86 |      X86 | 560.5 ms |  7.269 ms |  6.799 ms |   1.02 |     0.02 |
| &#39;5000 1 remote csharp&#39; | LegacyJitX86 |      X86 | 902.6 ms | 17.958 ms | 33.286 ms |   1.65 |     0.06 |
