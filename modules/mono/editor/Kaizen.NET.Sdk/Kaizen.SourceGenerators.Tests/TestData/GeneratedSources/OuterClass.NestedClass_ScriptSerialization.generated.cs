using Kaizen;
using Kaizen.NativeInterop;

partial struct OuterClass
{
partial class NestedClass
{
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void SaveKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.SaveKaizenObjectData(info);
    }
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void RestoreKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.RestoreKaizenObjectData(info);
    }
}
}
