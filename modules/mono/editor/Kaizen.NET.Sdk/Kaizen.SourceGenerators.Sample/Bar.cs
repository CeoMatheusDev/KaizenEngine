namespace Kaizen.SourceGenerators.Sample
{
    public partial class Bar : KaizenObject
    {
    }

    // Foo in another file
    public partial class Foo
    {
    }

    public partial class NotSameNameAsFile : KaizenObject
    {
    }
}
