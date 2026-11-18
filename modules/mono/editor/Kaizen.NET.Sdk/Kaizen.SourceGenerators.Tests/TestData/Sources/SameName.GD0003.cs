using Kaizen;

namespace NamespaceA
{
    partial class SameName : KaizenObject
    {
        private int _field;
    }
}

// SameName again but different namespace
namespace NamespaceB
{
    partial class {|GD0003:SameName|} : KaizenObject
    {
        private int _field;
    }
}
