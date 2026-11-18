using Kaizen;

// This works because it inherits from KaizenObject.
[GlobalClass]
public partial class CustomGlobalClass1 : KaizenObject
{

}

// This works because it inherits from an object that inherits from KaizenObject
[GlobalClass]
public partial class CustomGlobalClass2 : Node
{

}

// This raises a GD0401 diagnostic error: global classes must inherit from KaizenObject
[GlobalClass]
public partial class {|GD0401:CustomGlobalClass3|}
{

}
