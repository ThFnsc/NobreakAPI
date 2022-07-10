namespace ThFnsc.Nobreak.Core.Models;

[Flags]
internal enum NobreakStatusFlags : byte
{
    Beep = 1 << 0,
    UnkB1 = 1 << 1,
    TestExecuting = 1 << 2,
    BatteryHealthy = 1 << 3,
    UnkB4 = 1 << 4,
    UnkB5 = 1 << 5,
    UnkB6 = 1 << 6,
    OnBattery = 1 << 7,
}