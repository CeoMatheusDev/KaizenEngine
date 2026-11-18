using Kaizen;
using Kaizen.Collections;

public partial class ExportDiagnostics_GD0107_OK : Node
{
    [Export]
    public Node NodeField;

    [Export]
    public Node[] SystemArrayOfNodesField;

    [Export]
    public Array<Node> KaizenArrayOfNodesField;

    [Export]
    public Dictionary<Node, string> KaizenDictionaryWithNodeAsKeyField;

    [Export]
    public Dictionary<string, Node> KaizenDictionaryWithNodeAsValueField;

    [Export]
    public Node NodeProperty { get; set; }

    [Export]
    public Node[] SystemArrayOfNodesProperty { get; set; }

    [Export]
    public Array<Node> KaizenArrayOfNodesProperty { get; set; }

    [Export]
    public Dictionary<Node, string> KaizenDictionaryWithNodeAsKeyProperty { get; set; }

    [Export]
    public Dictionary<string, Node> KaizenDictionaryWithNodeAsValueProperty { get; set; }
}

public partial class ExportDiagnostics_GD0107_KO : Resource
{
    [Export]
    public Node {|GD0107:NodeField|};

    [Export]
    public Node[] {|GD0107:SystemArrayOfNodesField|};

    [Export]
    public Array<Node> {|GD0107:KaizenArrayOfNodesField|};

    [Export]
    public Dictionary<Node, string> {|GD0107:KaizenDictionaryWithNodeAsKeyField|};

    [Export]
    public Dictionary<string, Node> {|GD0107:KaizenDictionaryWithNodeAsValueField|};

    [Export]
    public Node {|GD0107:NodeProperty|} { get; set; }

    [Export]
    public Node[] {|GD0107:SystemArrayOfNodesProperty|} { get; set; }

    [Export]
    public Array<Node> {|GD0107:KaizenArrayOfNodesProperty|} { get; set; }

    [Export]
    public Dictionary<Node, string> {|GD0107:KaizenDictionaryWithNodeAsKeyProperty|} { get; set; }

    [Export]
    public Dictionary<string, Node> {|GD0107:KaizenDictionaryWithNodeAsValueProperty|} { get; set; }
}
