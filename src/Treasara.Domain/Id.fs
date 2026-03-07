namespace Treasara.Domain

open NUlid

/// <summary>
/// Represents a strongly-typed unique identifier using ULID (Universally Unique Lexicographically Sortable Identifier).
/// </summary>
/// <typeparam name="'a">The type this identifier is associated with.</typeparam>
/// <remarks>
/// ULIDs provide several advantages over traditional GUIDs:
/// <list type="bullet">
/// <item><description>Lexicographically sortable</description></item>
/// <item><description>Timestamp-based ordering</description></item>
/// <item><description>Case-insensitive and URL-safe encoding</description></item>
/// <item><description>128-bit compatibility with UUIDs</description></item>
/// </list>
/// The generic type parameter enables compile-time type safety by preventing accidental
/// mixing of identifiers from different domain entities.
/// </remarks>
type Id<'a> = Id of Ulid

/// <summary>
/// Module containing functions for creating and manipulating strongly-typed identifiers.
/// </summary>
module Id =

    /// <summary>
    /// Creates a new unique identifier for the specified type.
    /// </summary>
    /// <typeparam name="'a">The type this identifier will be associated with.</typeparam>
    /// <returns>A new unique identifier containing a freshly generated ULID.</returns>
    /// <remarks>
    /// The generated ULID includes a timestamp component (48 bits) and random component (80 bits),
    /// ensuring both uniqueness and sortability.
    /// </remarks>
    let create<'a> () : Id<'a> = Id(Ulid.NewUlid())

    /// <summary>
    /// Extracts the underlying ULID value from a strongly-typed identifier.
    /// </summary>
    /// <param name="Id">The strongly-typed identifier to unwrap.</param>
    /// <returns>The underlying ULID value.</returns>
    let value (Id id) = id

    /// <summary>
    /// Converts a strongly-typed identifier to its string representation.
    /// </summary>
    /// <param name="id">The identifier to convert.</param>
    /// <returns>The string representation of the ULID (26 characters, Base32 encoded).</returns>
    /// <remarks>
    /// The resulting string is lexicographically sortable and maintains the temporal ordering
    /// of the original ULID.
    /// </remarks>
    let toString id = value id |> string