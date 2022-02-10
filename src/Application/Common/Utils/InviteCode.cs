namespace JahnDigital.StudentBank.Application.Common.Utils;

/// <summary>
///     Simple utility class to generate a random invite code of a given length.
/// </summary>
public static class InviteCode
{
    /// <summary>
    ///     List of valid characters for invite codes.
    /// </summary>
    private const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    ///     Create a new invite code of the specified length and return it.
    /// </summary>
    /// <param name="length"></param>
    public static string NewCode(int length = 6)
    {
        char[] chars = new char[length];
        Random rng = new();

        for (int i = 0; i < length; i++)
        {
            chars[i] = ValidChars[rng.Next(0, ValidChars.Length)];
        }

        return new string(chars);
    }
}
