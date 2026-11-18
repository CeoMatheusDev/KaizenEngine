namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This resource holds a set of cells to help bulk manipulations of <see cref="Kaizen.TileMap"/>.</para>
/// <para>A pattern always starts at the <c>(0, 0)</c> coordinates and cannot have cells with negative coordinates.</para>
/// </summary>
public partial class TileMapPattern : Resource
{
    private static readonly System.Type CachedType = typeof(TileMapPattern);

    private static readonly StringName NativeName = "TileMapPattern";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public TileMapPattern() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal TileMapPattern(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal TileMapPattern(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCell, 2224802556ul);

    /// <summary>
    /// <para>Sets the tile identifiers for the cell at coordinates <paramref name="coords"/>. See <see cref="Kaizen.TileMap.SetCell(int, Vector2I, int, Nullable{Vector2I}, int)"/>.</para>
    /// </summary>
    /// <param name="atlasCoords">If the parameter is null, then the default value is <c>new Vector2I(-1, -1)</c>.</param>
    public unsafe void SetCell(Vector2I coords, int sourceId = -1, Nullable<Vector2I> atlasCoords = null, int alternativeTile = -1)
    {
        Vector2I atlasCoordsOrDefVal = atlasCoords.HasValue ? atlasCoords.Value : new Vector2I(-1, -1);
        NativeCalls.kaizen_icall_4_1499(MethodBind0, KaizenObject.GetPtr(this), &coords, sourceId, &atlasCoordsOrDefVal, alternativeTile);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasCell, 3900751641ul);

    /// <summary>
    /// <para>Returns whether the pattern has a tile at the given coordinates.</para>
    /// </summary>
    public unsafe bool HasCell(Vector2I coords)
    {
        bool ret = NativeCalls.kaizen_icall_1_41(MethodBind1, KaizenObject.GetPtr(this), &coords).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveCell, 4153096796ul);

    /// <summary>
    /// <para>Remove the cell at the given coordinates.</para>
    /// </summary>
    public unsafe void RemoveCell(Vector2I coords, bool updateSize)
    {
        NativeCalls.kaizen_icall_2_42(MethodBind2, KaizenObject.GetPtr(this), &coords, updateSize.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCellSourceId, 2485466453ul);

    /// <summary>
    /// <para>Returns the tile source ID of the cell at <paramref name="coords"/>.</para>
    /// </summary>
    public unsafe int GetCellSourceId(Vector2I coords)
    {
        int ret = NativeCalls.kaizen_icall_1_438(MethodBind3, KaizenObject.GetPtr(this), &coords);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCellAtlasCoords, 3050897911ul);

    /// <summary>
    /// <para>Returns the tile atlas coordinates ID of the cell at <paramref name="coords"/>.</para>
    /// </summary>
    public unsafe Vector2I GetCellAtlasCoords(Vector2I coords)
    {
        Vector2I ret = NativeCalls.kaizen_icall_1_1500(MethodBind4, KaizenObject.GetPtr(this), &coords);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCellAlternativeTile, 2485466453ul);

    /// <summary>
    /// <para>Returns the tile alternative ID of the cell at <paramref name="coords"/>.</para>
    /// </summary>
    public unsafe int GetCellAlternativeTile(Vector2I coords)
    {
        int ret = NativeCalls.kaizen_icall_1_438(MethodBind5, KaizenObject.GetPtr(this), &coords);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUsedCells, 3995934104ul);

    /// <summary>
    /// <para>Returns the list of used cell coordinates in the pattern.</para>
    /// </summary>
    public Kaizen.Collections.Array<Vector2I> GetUsedCells()
    {
        Kaizen.Collections.Array<Vector2I> ret = new Kaizen.Collections.Array<Vector2I>(NativeCalls.kaizen_icall_0_146(MethodBind6, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSize, 3690982128ul);

    /// <summary>
    /// <para>Returns the size, in cells, of the pattern.</para>
    /// </summary>
    public Vector2I GetSize()
    {
        Vector2I ret = NativeCalls.kaizen_icall_0_35(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSize, 1130785943ul);

    /// <summary>
    /// <para>Sets the size of the pattern.</para>
    /// </summary>
    public unsafe void SetSize(Vector2I size)
    {
        NativeCalls.kaizen_icall_1_34(MethodBind8, KaizenObject.GetPtr(this), &size);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsEmpty, 36873697ul);

    /// <summary>
    /// <para>Returns whether the pattern is empty or not.</para>
    /// </summary>
    public bool IsEmpty()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// Invokes the method with the given name, using the given arguments.
    /// This method is used by Kaizen to invoke methods from the engine side.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to invoke.</param>
    /// <param name="args">Arguments to use with the invoked method.</param>
    /// <param name="ret">Value returned by the invoked method.</param>
#pragma warning disable CS0618 // Member is obsolete
    protected internal override bool InvokeKaizenClassMethod(in kaizen_string_name method, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        return base.InvokeKaizenClassMethod(method, args, out ret);
    }
#pragma warning restore CS0618

    /// <summary>
    /// Check if the type contains a method with the given name.
    /// This method is used by Kaizen to check if a method exists before invoking it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to check for.</param>

    protected internal override bool HasKaizenClassMethod(in kaizen_string_name method)
    {
        return base.HasKaizenClassMethod(method);
    }

    /// <summary>
    /// Check if the type contains a signal with the given name.
    /// This method is used by Kaizen to check if a signal exists before raising it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="signal">Name of the signal to check for.</param>

    protected internal override bool HasKaizenClassSignal(in kaizen_string_name signal)
    {
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : Resource.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_cell' method.
        /// </summary>
        public static readonly StringName SetCell = "set_cell";
        /// <summary>
        /// Cached name for the 'has_cell' method.
        /// </summary>
        public static readonly StringName HasCell = "has_cell";
        /// <summary>
        /// Cached name for the 'remove_cell' method.
        /// </summary>
        public static readonly StringName RemoveCell = "remove_cell";
        /// <summary>
        /// Cached name for the 'get_cell_source_id' method.
        /// </summary>
        public static readonly StringName GetCellSourceId = "get_cell_source_id";
        /// <summary>
        /// Cached name for the 'get_cell_atlas_coords' method.
        /// </summary>
        public static readonly StringName GetCellAtlasCoords = "get_cell_atlas_coords";
        /// <summary>
        /// Cached name for the 'get_cell_alternative_tile' method.
        /// </summary>
        public static readonly StringName GetCellAlternativeTile = "get_cell_alternative_tile";
        /// <summary>
        /// Cached name for the 'get_used_cells' method.
        /// </summary>
        public static readonly StringName GetUsedCells = "get_used_cells";
        /// <summary>
        /// Cached name for the 'get_size' method.
        /// </summary>
        public static readonly StringName GetSize = "get_size";
        /// <summary>
        /// Cached name for the 'set_size' method.
        /// </summary>
        public static readonly StringName SetSize = "set_size";
        /// <summary>
        /// Cached name for the 'is_empty' method.
        /// </summary>
        public static readonly StringName IsEmpty = "is_empty";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
