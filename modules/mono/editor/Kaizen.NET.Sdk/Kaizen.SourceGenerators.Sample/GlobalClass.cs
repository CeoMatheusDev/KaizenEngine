namespace Kaizen.SourceGenerators.Sample;

[GlobalClass]
public partial class CustomGlobalClass : KaizenObject
{
}

// This doesn't works because global classes can't have any generic type parameter.
/*
[GlobalClass]
public partial class CustomGlobalClass<T> : Node
{
}
*/
