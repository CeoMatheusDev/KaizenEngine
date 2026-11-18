using Kaizen;
using Kaizen.NativeInterop;

partial class InheritanceChild
{
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void SaveKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.SaveKaizenObjectData(info);
        info.AddProperty(PropertyName.@MyString, global::Kaizen.Variant.From<string>(this.@MyString));
        info.AddProperty(PropertyName.@MyInteger, global::Kaizen.Variant.From<int>(this.@MyInteger));
    }
    /// <inheritdoc/>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void RestoreKaizenObjectData(global::Kaizen.Bridge.KaizenSerializationInfo info)
    {
        base.RestoreKaizenObjectData(info);
        if (info.TryGetProperty(PropertyName.@MyString, out var _value_MyString))
            this.@MyString = _value_MyString.As<string>();
        if (info.TryGetProperty(PropertyName.@MyInteger, out var _value_MyInteger))
            this.@MyInteger = _value_MyInteger.As<int>();
    }
}
