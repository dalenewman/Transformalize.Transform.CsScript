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
|       &#39;5000 test rows&#39; | LegacyJitX64 |      X64 | 468.0 ms |  5.057 ms |  4.483 ms |   1.00 |     0.00 |
|  &#39;5000 1 local csharp&#39; | LegacyJitX64 |      X64 | 478.3 ms |  5.347 ms |  5.001 ms |   1.02 |     0.01 |
| &#39;5000 1 remote csharp&#39; | LegacyJitX64 |      X64 | 747.4 ms |  5.559 ms |  4.928 ms |   1.60 |     0.02 |
|                        |              |          |          |           |           |        |          |
|       &#39;5000 test rows&#39; | LegacyJitX86 |      X86 | 513.8 ms | 12.960 ms | 12.123 ms |   1.00 |     0.00 |
|  &#39;5000 1 local csharp&#39; | LegacyJitX86 |      X86 | 515.9 ms |  4.151 ms |  3.883 ms |   1.00 |     0.02 |
| &#39;5000 1 remote csharp&#39; | LegacyJitX86 |      X86 | 789.3 ms |  8.761 ms |  7.767 ms |   1.54 |     0.04 |
