**WARNING**: This is a work in progress.

---

This adds C# transform to Transformalize using [CS-Script](https://github.com/oleg-shilo/cs-script).  It is a plug-in compatible with Transformalize 0.3.2-beta.

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

Note: Eventually this library will allow you to set a `remote` attribute on the field to `true`. This lets you avoid memory 
leaks associated with running dyanamically loaded c# assemblies in the host's `AppDomain`.  Unfortunately, 
initial tests show this moving code to remote `AppDomain` introduces a huge performance issue.