namespace MultipleScreenPowersave.Extensions;

using System;
using System.Collections.Generic;
using CommunityToolkit.Diagnostics;
using Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
/// Contains method for formatting combined enum values likw dwStyle or dwExStyle as string.
/// </summary>
public static class UInt32Extensions
{
    /// <summary>
    /// Format a uint value resulting from a combination of WINDOW_STYLE enum values as string.
    /// </summary>
    /// <param name="value">Value to format.</param>
    /// <returns>String representation of value.</returns>
    public static string WindowStyleToString(this uint value)
    {
        var constants = new List<string>();
        var remainingValue = value;

        var enumValuesDescending = Enum.GetValues<WINDOW_STYLE>().ToHashSet().ToList();
        enumValuesDescending.Sort(
            (it1, it2) =>
            {
                return (it1, it2) switch
                {
                    var tuple when tuple.it2 < tuple.it1 => -1,
                    var tuple when tuple.it2 > tuple.it1 => 1,
                    _ => 0,
                };
            }
        );

        bool identifiedConstant = true;
        while (remainingValue > 0 && identifiedConstant)
        {
            identifiedConstant = false;

            switch (remainingValue)
            {
                case var v
                    when (v & (uint)WINDOW_STYLE.WS_TILEDWINDOW)
                        == (uint)WINDOW_STYLE.WS_TILEDWINDOW:
                    remainingValue -= (uint)WINDOW_STYLE.WS_TILEDWINDOW;
                    constants.Add(nameof(WINDOW_STYLE.WS_TILEDWINDOW));
                    identifiedConstant = true;
                    break;

                case var v
                    when (v & (uint)WINDOW_STYLE.WS_POPUPWINDOW)
                        == (uint)WINDOW_STYLE.WS_POPUPWINDOW:
                    remainingValue -= (uint)WINDOW_STYLE.WS_POPUPWINDOW;
                    constants.Add(nameof(WINDOW_STYLE.WS_POPUPWINDOW));
                    identifiedConstant = true;
                    break;

                case var v
                    when (v & (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW)
                        == (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW:
                    remainingValue -= (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
                    constants.Add(nameof(WINDOW_STYLE.WS_OVERLAPPEDWINDOW));
                    identifiedConstant = true;
                    break;

                default:
                    foreach (WINDOW_STYLE enumValue in enumValuesDescending)
                    {
                        if ((uint)enumValue == 0)
                            continue;

                        if ((remainingValue & (uint)enumValue) == (uint)enumValue)
                        {
                            remainingValue -= (uint)enumValue;
                            constants.Add(Enum.GetName(enumValue)!);
                            identifiedConstant = true;
                            break;
                        }
                    }

                    break;
            }
        }

        if (remainingValue > 0)
            constants.Add(remainingValue.ToHexString());

        return string.Join(" | ", constants);
    }

    /// <summary>
    /// Format a uint value resulting from a combination of WINDOW_EX_STYLE enum values as string.
    /// </summary>
    /// <param name="value">Value to format.</param>
    /// <returns>String representation of value.</returns>
    public static string ExtendedWindowStyleToString(this uint value)
    {
        var constants = new List<string>();
        var remainingValue = value;

        var enumValuesDescending = Enum.GetValues<WINDOW_EX_STYLE>().ToHashSet().ToList();
        enumValuesDescending.Sort(
            (it1, it2) =>
            {
                return (it1, it2) switch
                {
                    var tuple when tuple.it2 < tuple.it1 => -1,
                    var tuple when tuple.it2 > tuple.it1 => 1,
                    _ => 0,
                };
            }
        );

        bool identifiedConstant = true;
        while (remainingValue > 0 && identifiedConstant)
        {
            identifiedConstant = false;

            switch (remainingValue)
            {
                case var v
                    when (v & (uint)WINDOW_EX_STYLE.WS_EX_PALETTEWINDOW)
                        == (uint)WINDOW_EX_STYLE.WS_EX_PALETTEWINDOW:
                    remainingValue -= (uint)WINDOW_EX_STYLE.WS_EX_PALETTEWINDOW;
                    constants.Add(nameof(WINDOW_EX_STYLE.WS_EX_PALETTEWINDOW));
                    identifiedConstant = true;
                    break;

                case var v
                    when (v & (uint)WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW)
                        == (uint)WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW:
                    remainingValue -= (uint)WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW;
                    constants.Add(nameof(WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW));
                    identifiedConstant = true;
                    break;

                default:
                    foreach (WINDOW_EX_STYLE enumValue in enumValuesDescending)
                    {
                        if ((uint)enumValue == 0)
                            continue;

                        if ((remainingValue & (uint)enumValue) == (uint)enumValue)
                        {
                            remainingValue -= (uint)enumValue;
                            constants.Add(Enum.GetName(enumValue)!);
                            identifiedConstant = true;
                            break;
                        }
                    }

                    break;
            }
        }

        if (remainingValue > 0)
            constants.Add(remainingValue.ToHexString());

        return string.Join(" | ", constants);
    }
}
