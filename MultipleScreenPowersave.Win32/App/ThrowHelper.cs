namespace MultipleScreenPowersave.App;

using System.Runtime.InteropServices;

/// <summary>
/// Helper class to throw last error (win32 api) as exception.
/// </summary>
public class ThrowHelper
{
    /// <summary>
    /// Throws an exception based on the current error code returned by (win32 api) GetLastError().
    /// </summary>
    public static void ThrowLastError()
    {
        var lastError = Marshal.GetLastWin32Error();
        Marshal.ThrowExceptionForHR(lastError);
    }

    /// <summary>
    /// Throws an exception based on the current error code returned by (win32 api) GetLastError()
    /// if the result passed in is 0.
    /// </summary>
    /// <param name="result">Result value to consider.</param>
    public static void ThrowLastErrorIfResultIsZero(int result)
    {
        if (result != 0u)
            return;

        ThrowLastError();
    }

    /// <summary>
    /// Throws an exception based on the current error code returned by (win32 api) GetLastError()
    /// if the result passed in is 0.
    /// </summary>
    /// <param name="result">Result value to consider.</param>
    public static void ThrowLastErrorIfResultIsZero(uint result)
    {
        if (result != 0u)
            return;

        ThrowLastError();
    }
}
