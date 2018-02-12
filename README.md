### Overview

This adds C# transform to Transformalize using [CS-Script](https://github.com/oleg-shilo/cs-script).  It is a plug-in compatible with Transformalize 0.3.2-beta.

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
                <add name="csharped" t='cs(return text + " " + number;)' />
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

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2742192 Hz, Resolution=364.6718 ns, Timer=TSC
  [Host]       : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2600.0
  LegacyJitX64 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2600.0;compatjit-v4.7.2600.0
  LegacyJitX86 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2600.0

Jit=LegacyJit  Runtime=Clr  

```
|                        Method |          Job | Platform |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|------------------------------ |------------- |--------- |----------:|----------:|----------:|-------:|---------:|
|               &#39;500 test rows&#39; | LegacyJitX64 |      X64 |  65.20 ms | 0.3868 ms | 0.3619 ms |   1.00 |     0.00 |
| &#39;500 local csharp transforms&#39; | LegacyJitX64 |      X64 |  73.45 ms | 0.7392 ms | 0.6915 ms |   1.13 |     0.01 |
| &#39;500 remote csharp transforms&#39; | LegacyJitX64 |      X64 | 134.13 ms | 0.8207 ms | 0.7677 ms |   2.06 |     0.02 |
|                               |              |          |           |           |           |        |          |
|               &#39;500 test rows&#39; | LegacyJitX86 |      X86 |  70.14 ms | 0.2699 ms | 0.2525 ms |   1.00 |     0.00 |
| &#39;500 local csharp transforms&#39; | LegacyJitX86 |      X86 |  73.44 ms | 0.6242 ms | 0.5839 ms |   1.05 |     0.01 |
| &#39;500 remote csharp transforms&#39; | LegacyJitX86 |      X86 | 131.84 ms | 0.5281 ms | 0.4682 ms |   1.88 |     0.01 |
