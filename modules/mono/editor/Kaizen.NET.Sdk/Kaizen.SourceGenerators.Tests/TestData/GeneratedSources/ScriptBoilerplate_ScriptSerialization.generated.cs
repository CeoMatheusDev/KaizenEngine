using Kaizen;
using Kaizen.NativeInterop;

partial class ScriptBoilerplate
{
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void SaveKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.SaveKaizenObjectData(info);
        info.AddProperty(PropertyName.@_nodePath, global::Kaizen.Variant.From<global::Kaizen.NodePath>(this.@_nodePath));
        info.AddProperty(PropertyName.@_velocity, global::Kaizen.Variant.From<int>(this.@_velocity));
    }
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void RestoreKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.RestoreKaizenObjectData(info);
        if (info.TryGetProperty(PropertyName.@_nodePath, out var _value__nodePath))
            this.@_nodePath = _value__nodePath.As<global::Kaizen.NodePath>();
        if (info.TryGetProperty(PropertyName.@_velocity, out var _value__velocity))
            this.@_velocity = _value__velocity.As<int>();
    }
}
