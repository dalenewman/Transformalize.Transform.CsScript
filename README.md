### Overview

This adds C# transform to Transformalize using [CS-Script](https://github.com/oleg-shilo/cs-script).  It is a plug-in compatible with Transformalize 0.6.1-beta.

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

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742192 Hz, Resolution=364.6718 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3221.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3221.0;compatjit-v4.7.3221.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|                  Method |       Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------ |-----------:|----------:|----------:|------:|--------:|
|       &#39;10000 test rows&#39; |   916.0 ms | 11.914 ms |  9.949 ms |  1.00 |    0.00 |
|  &#39;10000 1 local csharp&#39; |   957.5 ms | 18.817 ms | 21.669 ms |  1.05 |    0.03 |
| &#39;10000 1 remote csharp&#39; | 1,483.2 ms |  8.235 ms |  7.300 ms |  1.62 |    0.02 |
