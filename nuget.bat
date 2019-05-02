nuget pack Transformalize.Transform.CsScript.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.CsScript.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.0.6.2-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.Autofac.0.6.2-beta.nupkg" -source https://api.nuget.org/v3/index.json






