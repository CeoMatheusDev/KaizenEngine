using System;

namespace Kaizen.SourceGenerators.Sample;

public partial class NestedClass : KaizenObject
{
    public partial class NestedClass2 : KaizenObject
    {
        public partial class NestedClass3 : KaizenObject
        {
            [Signal]
            public delegate void MySignalEventHandler(string str, int num);

            [Export] private String _fieldString = "foo";
            [Export] private String PropertyString { get; set; } = "foo";

            private void Method()
            {
            }
        }
    }
}
