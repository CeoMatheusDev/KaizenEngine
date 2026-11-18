using Kaizen;

public partial class AllWriteOnly : KaizenObject
{
    private bool _writeOnlyBackingField = false;
    public bool WriteOnlyProperty { set => _writeOnlyBackingField = value; }
}
