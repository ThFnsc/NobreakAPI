using ThFnsc.Nobreak.Core.Models;

namespace ThFnsc.Nobreak.Core.Services;

public interface INobreakCommunicator
{
    bool Connected { get; }
    void Connect();
    void Disconnect();
    NobreakStatus GetStatus();
    NobreakStatus Test(byte? minutes = null);
    NobreakStatus TestUntilFlatBattery();
    NobreakStatus CancelTest();
}
