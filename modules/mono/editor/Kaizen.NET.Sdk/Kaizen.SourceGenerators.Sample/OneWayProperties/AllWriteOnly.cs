using System;

namespace Kaizen.SourceGenerators.Sample
{
    public partial class AllWriteOnly : KaizenObject
    {
        private bool _writeOnlyBackingField = false;
        public bool WriteOnlyProperty { set => _writeOnlyBackingField = value; }
    }
}
