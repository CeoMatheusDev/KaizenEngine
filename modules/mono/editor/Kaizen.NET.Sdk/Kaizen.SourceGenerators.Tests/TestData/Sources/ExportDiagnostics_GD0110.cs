using Kaizen;
using Kaizen.Collections;

[Tool]
public partial class ExportDiagnostics_GD0110 : Node
{
    [ExportToolButton("")]
    public int {|GD0110:MyButton|} => new();
}
