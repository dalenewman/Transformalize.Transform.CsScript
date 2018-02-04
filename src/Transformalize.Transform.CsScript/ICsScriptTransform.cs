namespace Transformalize.Transforms.CsScript {
    public interface ICsScriptTransform {
        object Transform(object[] rowData);
    }
}