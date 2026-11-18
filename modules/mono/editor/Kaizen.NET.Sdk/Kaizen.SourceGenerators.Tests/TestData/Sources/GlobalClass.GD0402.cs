using Kaizen;

// This works because it inherits from KaizenObject and it doesn't have any generic type parameter.
[GlobalClass]
public partial class CustomGlobalClass : KaizenObject
{

}

// This raises a GD0402 diagnostic error: global classes can't have any generic type parameter
[GlobalClass]
public partial class {|GD0402:CustomGlobalClass|}<T> : KaizenObject
{

}
