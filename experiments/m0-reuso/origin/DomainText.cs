namespace ClubAL.Domain;

internal static class DomainText
{
    public static string Required(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"O valor deve conter no máximo {maximumLength} caracteres.");
        }

        return normalized;
    }
}
