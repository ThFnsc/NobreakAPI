using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Ports;
using ThFnsc.Nobreak.Core.Models;

namespace ThFnsc.Nobreak.Core.Services;

public class NobreakCommunicator : INobreakCommunicator, IDisposable
{
    private SerialPort? _serialPort;

    public bool Connected
    {
        get
        {
            lock (this)
                return ConnectedInternal;
        }
    }

    private bool ConnectedInternal => _serialPort?.IsOpen ?? false;

    public void Connect()
    {
        lock (this)
            ConnectInternal();
    }

    private void ConnectInternal()
    {
        if (Connected)
            return;
        DisconnectInternal();
        var exceptions = new List<Exception>();
        foreach (var port in SerialPort.GetPortNames().Reverse())
        {
            _serialPort = new SerialPort(port, 9600);
            try
            {
                _serialPort.WriteTimeout = 10;
                _serialPort.ReadTimeout = 500;
                _serialPort.Open();
                GetStatus();
                return;
            }
            catch (Exception e)
            {
                Disconnect();
                e = new Exception($"Failed to connect to serial port '{port}': {e.Message}", e);
                exceptions.Add(e);
            }
        }
        throw new AggregateException(exceptions);
    }

    public void Disconnect()
    {
        lock (this)
            DisconnectInternal();
    }

    private void DisconnectInternal()
    {
        if (_serialPort is null)
            return;
        if (_serialPort.IsOpen)
            _serialPort.Close();
        _serialPort.Dispose();
        _serialPort = null;
    }

    public NobreakStatus GetStatus()
    {
        lock (this)
            return GetStatusInternal();
    }

    private NobreakStatus GetStatusInternal()
    {
        Write("Q1");
        var response = Read();
        return NobreakStatus.ParseFromSerialResponse(response);
    }

    private void Write(string text)
    {
        if (!ConnectedInternal)
            ConnectInternal();
        _serialPort!.Write(text + '\r');
    }

    private string Read()
    {
        ArgumentNullException.ThrowIfNull(_serialPort);
        return _serialPort.ReadTo("\r");
    }

    public void Dispose() =>
        Disconnect();

    public NobreakStatus Test(byte? minutes = null)
    {
        if (minutes is 0 or > 99)
            throw new ArgumentException("If minutes set, cannot be 0 or greater than 99", nameof(minutes));
        lock (this)
        {
            if (minutes.HasValue)
                Write($"T{minutes:00}");
            Write("T");
            return EnsureTestIs(running: true);
        }
    }

    private NobreakStatus EnsureTestIs(bool running)
    {
        Thread.Sleep(100);
        var status = GetStatusInternal();
        if (status.TestExecuting != running)
            throw new Exception("Nobreak did not obey test command");
        return status;
    }

    public NobreakStatus TestUntilFlatBattery()
    {
        lock (this)
        {
            Write("TL");
            return EnsureTestIs(running: true);
        }
    }

    public NobreakStatus CancelTest()
    {
        lock (this)
        {
            Write("CT");
            return EnsureTestIs(running: false);
        }
    }

    public NobreakStatus SetBeep(bool enabled)
    {
        lock (this)
        {
            var status = GetStatusInternal();
            if (status.BeepOn == enabled)
                return status;
            Thread.Sleep(100);
            Write("Q");
            Thread.Sleep(100);
            status = GetStatusInternal();
            if (status.BeepOn != enabled)
                throw new Exception("Nobreak did not obey toggle beep command");
            return status;
        }
    }
}
