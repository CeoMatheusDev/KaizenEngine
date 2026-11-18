using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Kaizen.SourceGenerators
{
    public readonly struct KaizenMethodData
    {
        public KaizenMethodData(IMethodSymbol method, ImmutableArray<MarshalType> paramTypes,
            ImmutableArray<ITypeSymbol> paramTypeSymbols, (MarshalType MarshalType, ITypeSymbol TypeSymbol)? retType)
        {
            Method = method;
            ParamTypes = paramTypes;
            ParamTypeSymbols = paramTypeSymbols;
            RetType = retType;
        }

        public IMethodSymbol Method { get; }
        public ImmutableArray<MarshalType> ParamTypes { get; }
        public ImmutableArray<ITypeSymbol> ParamTypeSymbols { get; }
        public (MarshalType MarshalType, ITypeSymbol TypeSymbol)? RetType { get; }
    }

    public readonly struct KaizenSignalDelegateData
    {
        public KaizenSignalDelegateData(string name, INamedTypeSymbol delegateSymbol, KaizenMethodData invokeMethodData)
        {
            Name = name;
            DelegateSymbol = delegateSymbol;
            InvokeMethodData = invokeMethodData;
        }

        public string Name { get; }
        public INamedTypeSymbol DelegateSymbol { get; }
        public KaizenMethodData InvokeMethodData { get; }
    }

    public readonly struct KaizenPropertyData
    {
        public KaizenPropertyData(IPropertySymbol propertySymbol, MarshalType type)
        {
            PropertySymbol = propertySymbol;
            Type = type;
        }

        public IPropertySymbol PropertySymbol { get; }
        public MarshalType Type { get; }

        public bool IsReadOnly => PropertySymbol.IsReadOnly ||
                                  PropertySymbol.SetMethodOrBaseSetMethod() is { IsInitOnly: true };
        public bool IsWriteOnly => PropertySymbol.IsWriteOnly;
    }

    public readonly struct KaizenFieldData
    {
        public KaizenFieldData(IFieldSymbol fieldSymbol, MarshalType type)
        {
            FieldSymbol = fieldSymbol;
            Type = type;
        }

        public IFieldSymbol FieldSymbol { get; }
        public MarshalType Type { get; }

        public bool IsReadOnly => FieldSymbol.IsReadOnly;
    }

    public struct KaizenPropertyOrFieldData
    {
        public KaizenPropertyOrFieldData(ISymbol symbol, MarshalType type)
        {
            Symbol = symbol;
            Type = type;
        }

        public KaizenPropertyOrFieldData(KaizenPropertyData propertyData)
            : this(propertyData.PropertySymbol, propertyData.Type)
        {
        }

        public KaizenPropertyOrFieldData(KaizenFieldData fieldData)
            : this(fieldData.FieldSymbol, fieldData.Type)
        {
        }

        public ISymbol Symbol { get; }
        public MarshalType Type { get; }
    }
}
