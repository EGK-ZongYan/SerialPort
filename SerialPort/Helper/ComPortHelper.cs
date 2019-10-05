using SerialPortLibrary.Data;
using SerialPortLibrary.Interface;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;

namespace SerialPortLibrary.Helper
{
    public class ComPortHelper : ISerialPortHelper<ComPort>
    {
        public List<ComPort> GetAllPorts()
        {
            // Method 2
            List<ComPort> ports = new List<ComPort>();

            // Method 1
            string[] ports2 = SerialPort.GetPortNames();
            foreach (string port in ports2)
            {
                // Only need to check the Port Name
                if (!ports.Exists(x => x.Name == port))
                {
                    ports.Add(new ComPort(port, port));
                }
            }
            return ports;
        }

        public List<ComPort> GetSerialPort()
        {
            List<ComPort> ports = new List<ComPort>();
            var searcher = new ManagementObjectSearcher("SELECT DeviceID,Caption FROM WIN32_SerialPort");
            foreach (ManagementObject port in searcher.Get())
            {
                // show the service
                ComPort c = new ComPort();
                c.Name = port.GetPropertyValue("DeviceID").ToString();
                c.Description = port.GetPropertyValue("Caption").ToString();
                ports.Add(c);
            }
            return ports;
        }
    }
}
