using System.Collections.Generic;

namespace SerialPortLibrary.Interface
{
    public interface ISerialPortHelper<T>
    {
        List<T> GetAllPorts();
        List<T> GetSerialPort();
    }
}
