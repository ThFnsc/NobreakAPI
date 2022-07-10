using System.Runtime.CompilerServices;

namespace ThFnsc.Nobreak.Core.Models;

public readonly struct NobreakStatus
{
    public float VoltageIn { get; init; }
    
    public float VoltageOut { get; init; }
    
    public byte LoadPercentage { get; init; }
    
    public float FrequencyHz { get; init; }
    
    public float BatteryVoltage { get; init; }
    
    public float TemperatureC { get; init; }
    
    public bool BeepOn { get; init; }
    
    public bool BatteryHealthy { get; init; }
    
    public PowerSource PowerSource { get; init; }

    public bool TestExecuting { get; init; }

    internal static NobreakStatus ParseFromSerialResponse(string serial)
    {
        ArgumentNullException.ThrowIfNull(serial);
        if (!serial.StartsWith('('))
            throw new FormatException("String does not begin with '('");
        
        var parts = serial.Substring(1).Split(' ');
        if (parts.Length != 8)
            throw new FormatException($"Expected 8 values separated by spaces. Got {parts.Length} instead");


        Parse(parts[0], float.TryParse, out float vIn1);
        Parse(parts[1], float.TryParse, out float vIn2);
        Parse(parts[2], float.TryParse, out float vOut);
        Parse(parts[3], byte.TryParse, out byte load);
        Parse(parts[4], float.TryParse, out float frequencyHz);
        Parse(parts[5], float.TryParse, out float vBat);
        Parse(parts[6], float.TryParse, out float tempC);
        Parse(parts[7], TryParseBinaryStringToByte, out byte flags);

        var nbFlags = (NobreakStatusFlags)flags;

        return new NobreakStatus
        {
            VoltageIn = vIn1,
            VoltageOut = vOut * 2,
            LoadPercentage = load,
            FrequencyHz = frequencyHz,
            BatteryVoltage = vBat,
            TemperatureC = tempC,
            BeepOn = nbFlags.HasFlag(NobreakStatusFlags.Beep),
            BatteryHealthy = nbFlags.HasFlag(NobreakStatusFlags.BatteryHealthy),
            TestExecuting = nbFlags.HasFlag(NobreakStatusFlags.TestExecuting),
            PowerSource = nbFlags.HasFlag(NobreakStatusFlags.OnBattery)
                ? PowerSource.Battery
                : PowerSource.Grid
        };
    }

    private static bool TryParseBinaryStringToByte(string input, out byte result)
    {
        try
        {
            result = Convert.ToByte(input, 2);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private delegate bool TryParser<T>(string name, out T value);

    private static void Parse<T>(string value, TryParser<T> parser, out T result, [CallerArgumentExpression("result")] string? desc = null)
    {
        if (!parser(value, out result))
            throw new FormatException($"Invalid '{desc}' string: '{value}'");
    }
}
