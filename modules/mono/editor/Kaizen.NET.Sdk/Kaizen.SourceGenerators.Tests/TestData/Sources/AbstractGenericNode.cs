using Kaizen;

public abstract partial class AbstractGenericNode<[MustBeVariant] T> : Node
{
    [Export] // This should be included, but without type hints.
    public Kaizen.Collections.Array<T> MyArray { get; set; } = new();
}
