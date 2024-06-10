// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.PublicApi;

internal class IgnoreCaseWhenPossibleComparer : IComparer<string>
{
    /// <summary>
    ///     Gets a <see cref="StringComparer" /> object that performs a case-insensitive ordinal string comparison first, then
    ///     a case-sensitive ordinal string comparison.
    /// </summary>
    public static readonly IgnoreCaseWhenPossibleComparer Instance = new();

    private IgnoreCaseWhenPossibleComparer() { }

    /// <inheritdoc />
    public int Compare(string? x, string? y)
    {
        var result = StringComparer.OrdinalIgnoreCase.Compare(x, y);

        if (result == 0)
        {
            result = StringComparer.Ordinal.Compare(x, y);
        }

        return result;
    }
}
