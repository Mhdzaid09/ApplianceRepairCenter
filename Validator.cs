using System.Text.RegularExpressions;

namespace ApplianceRepairCenter.Helpers;

/// <summary>
/// Static class that groups all input-validation helpers used by the system.
/// </summary>
public static class Validator
{
    // Serial format: two uppercase letters, a dash, then 4 to 8 digits (e.g. RF-10234)
    private static readonly Regex SerialPattern = new(@"^[A-Z]{2}-\d{4,8}$", RegexOptions.Compiled);

    public static bool IsValidSerialNumber(string? serial)
        => !string.IsNullOrWhiteSpace(serial) && SerialPattern.IsMatch(serial);

    public static bool IsValidName(string? text)
        => !string.IsNullOrWhiteSpace(text) && text.Trim().Length >= 2;

    public static bool IsNonNegative(decimal value) => value >= 0;

    public static bool IsNonNegative(double value) => value >= 0;

    /// <summary>
    /// The expected delivery date must not be earlier than the date the appliance was received.
    /// </summary>
    public static bool IsValidDeliveryDate(DateTime received, DateTime expectedDelivery)
        => expectedDelivery.Date >= received.Date;

    public static bool TryParseDate(string? input, out DateTime date)
        => DateTime.TryParse(input, out date);
}
