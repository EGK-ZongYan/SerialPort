using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerialPortLibrary.Data;
using SerialPortLibrary.Helper;
using SerialPortLibrary.Interface;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace ModbusPlatform
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="responseDataLength">回傳的資料長度</param>
        /// <returns></returns>
        public static byte[] ReadOfPort(this SerialPort serialPort , int responseDataLength)
        {
            var dataLength = responseDataLength;
            Byte[] buffer = new Byte[dataLength];

            var tryMaxCount = 5;
            var reTryCount = 0;

            while (serialPort.IsOpen && serialPort.BytesToRead < dataLength && reTryCount < tryMaxCount)
            {
                Thread.Sleep(1 * 1000);
                reTryCount++;
            }

            if (serialPort.BytesToRead > 0)
            {
                try
                {
                    int read = 0;
                    int len;
                    while (read < serialPort.BytesToRead)
                    {
                        len = serialPort.Read(buffer, 0, buffer.Length);
                        if (len == 0)
                            continue;
                        read += len;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error : " + e.ToString());
                }
            }

            serialPort.DiscardInBuffer();

            return buffer;
        }
    }
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static IConfiguration IConfiguration;
        private static string environmentName;
        private static SerialPort _serial_sphygmomanometer;

        static void Main(string[] args)
        {
            #region == SET CONFIG ==

            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings." + (string.IsNullOrEmpty(environmentName) ? "" : (environmentName + ".")) + "json", false)            
            .AddEnvironmentVariables();

            #endregion

            RegisterServices();

            Console.WriteLine("顯示ComPort：");

            var service = _serviceProvider.GetService<ISerialPortHelper<ComPort>>();

            var comPorts = service.GetAllPorts();

            if (comPorts.Count > 0)
            {
                foreach (var comPort in comPorts)
                {
                    Console.WriteLine($"{comPort.Name} - {comPort.Description}");
                }

                Console.WriteLine("Please choose ComPort：");

                var chooseComPort = Console.ReadLine();
                Console.WriteLine("-------------------------------------------------------------------------------------------------------");

                _serial_sphygmomanometer = new SerialPort();
                _serial_sphygmomanometer.PortName = chooseComPort;
                _serial_sphygmomanometer.BaudRate = 9600;               // Baudrate = 9600bps
                _serial_sphygmomanometer.Parity = Parity.None;        // Parity bits = none  
                _serial_sphygmomanometer.DataBits = 8;                  // No of Data bits = 8
                _serial_sphygmomanometer.StopBits = StopBits.One;       // No of Stop bits = 1

                int dataLength = 0;

                // 直接宣告
                //_serial_sphygmomanometer.ReadBufferSize = dataLength;
                //_serial_sphygmomanometer.WriteBufferSize = dataLength;
                //_serial_sphygmomanometer.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

                Console.WriteLine("Connect to Serial Port ......");

                _serial_sphygmomanometer.Open();

                Console.WriteLine(_serial_sphygmomanometer.IsOpen ? "Connected" : "not Open");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------");

                while (true)
                {
                    Console.WriteLine("Please Enter Command：[Restart]、[SearchAddress]、[GetChannelNumber]、[OpenChannel]、[GetChannelStatus]");

                    var command = Console.ReadLine();
                    Console.WriteLine("-------------------------------------------------------------------------------------------------------");

                    var api = new byte[0];

                    var commandIsExists = true;

                    var lockControlBoardNumber = -1;

                    Console.WriteLine("Please enter the 〝Lock control board number 〞(0 ~ 10)");

                    var isParseInt = int.TryParse(Console.ReadLine() , out lockControlBoardNumber);
                    Console.WriteLine("-------------------------------------------------------------------------------------------------------");

                    if (!isParseInt) break;
                    var hasEnterChannel = false;

                    switch (command)
                    {
                        //case "info":
                        //    api = API.GetMeasurement;
                        //    dataLength = 64;
                        //    break;
                        //case "start":
                        //    api = API.StartMeasurement;
                        //    dataLength = 64;
                        //    break;
                        //case "end":
                        //    api = API.StopMeasurement;
                        //    dataLength = 64;
                        //    break;                        
                        case "Restart":
                            api = API.Restart;
                            dataLength = 9;
                            break;
                        case "SearchAddress":
                            api = API.SearchAddress;
                            dataLength = 9;
                            break;
                        case "GetChannelNumber":
                            api = API.GetChannelNumber;
                            dataLength = 9;
                            break;
                        case "OpenChannel":
                            api = API.OpenChannel;
                            dataLength = 10;
                            hasEnterChannel = true;
                            break;
                        case "GetChannelStatus":
                            api = API.GetChannelStatus;
                            dataLength = 10;
                            hasEnterChannel = true;
                            break;
                        default:
                            commandIsExists = false;
                            break;
                    }

                    //設定發送端位址
                    api[3] = Convert.ToByte(lockControlBoardNumber);

                    if (hasEnterChannel)
                    {
                        Console.WriteLine("Please enter channels (1 ~ 10) : ");

                        int channelNumber;
                        isParseInt = int.TryParse(Console.ReadLine(), out channelNumber);
                        Console.WriteLine("-------------------------------------------------------------------------------------------------------");

                        api[8] = Convert.ToByte(channelNumber);

                        if (!isParseInt) break;
                    }

                    if (commandIsExists)
                    {
                        try
                        {                            
                            var stringAPI = "";

                            foreach (var value in api)
                            {
                                stringAPI += string.Format("{0:X2}",value) + " ";
                            }

                            Console.WriteLine("Send API:" + stringAPI);
                            
                            _serial_sphygmomanometer.Write(api, 0, api.Length);

                            byte[] resultArray = _serial_sphygmomanometer.ReadOfPort(dataLength);

                            string resultStr = System.Text.Encoding.UTF8.GetString(resultArray);

                            Console.WriteLine("return result : " + resultStr);
                            
                        }
                        catch (global::System.Exception e)
                        {
                            Console.WriteLine("error : " + e.ToString());
                        }
                        finally {

                        }                                            
                    }
                    else {
                        Console.WriteLine("The command you entered is incorrect. Please re-enter");
                    }

                    Console.WriteLine("====================================== END ======================================");
                }                
            }
            else {
                Console.WriteLine("not found the ComPort");
            }

            Console.WriteLine("Please press any key to end the program");
            Console.ReadKey();

            DisposeServices();
        }

        public class API
        {
            /// <summary>
            /// 取得數值
            /// </summary>
            public static byte[] GetMeasurement = new byte[10] { 0x16, 0x16, 0x01, 0x30, 0x30, 0x02, 0x52, 0x42, 0x03, 0x10 };

            /// <summary>
            /// 開始測量
            /// </summary>
            public static byte[] StartMeasurement = new byte[10] { 0x16, 0x16, 0x01, 0x30, 0x30, 0x02, 0x52, 0x43, 0x03, 0x10 };

            /// <summary>
            /// 停止測量
            /// </summary>
            public static byte[] StopMeasurement = new byte[10] { 0x16, 0x16, 0x01, 0x30, 0x30, 0x02, 0x52, 0x44, 0x03, 0x10 };


            //智慧鑰匙櫃
            /*
                [0]  - 幀頭 固定 0x5A
                [1]  - 幀頭 固定 0x5A
                [2]  - 原地址 主機發送命令 0x00 鎖控板回答為 0x00 ~ 0x31
                [3]  - 目的地址 主機發送命令 0x00 ~ 0x31 鎖控板回答為 0x00
                [4]  - 包序號 0x00
                [5]  - 命令
                [6]  - 結果 固定 0x00
                [7]  - 數據長度
                [8]  - 數據
                [9]  - 校驗
            */

            //0x01
            public static byte[] Restart            = new byte[9]   { 0x5A, 0x5A, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00 };

            //0x02
            public static byte[] SearchAddress      = new byte[9]   { 0x5A, 0x5A, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x03 };

            //0x03
            public static byte[] GetChannelNumber   = new byte[9]   { 0x5A, 0x5A, 0x00, 0x01, 0x00, 0x03, 0x00, 0x00, 0x02 };

            //0x04
            public static byte[] OpenChannel        = new byte[10]  { 0x5A, 0x5A, 0x00, 0x01, 0x00, 0x04, 0x00, 0x01, 0x01, 0x05 };

            //0x06
            public static byte[] GetChannelStatus   = new byte[10]  { 0x5A, 0x5A, 0x00, 0x01, 0x00, 0x06, 0x00, 0x01, 0x01, 0x07 };
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton(provider => IConfiguration);
            collection.AddScoped<ISerialPortHelper<ComPort>, ComPortHelper>();
            _serviceProvider = collection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
