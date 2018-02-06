**WARNING**: This is a work in progress.  It leaks more memory than the old CSharp provider.  Don't use.

---

This adds C# transform to Transformalize using [CS-Script](https://github.com/oleg-shilo/cs-script).  It is a plug-in compatible with Transformalize 0.3.1-beta.

Build the Autofac project and put it's output into Transformalize's *plugins* folder.

Use like this:

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

Note: You can set a `remote` attribute on the field to `true` if you want to avoid memory leaks associated 
with running dyanamically loaded c# assemblies into the host's `AppDomain`.