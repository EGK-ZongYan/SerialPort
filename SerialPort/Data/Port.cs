using System;
using System.Collections.Generic;
using System.Text;

namespace SerialPortLibrary.Data
{
    public class ComPort
    {
        public string Name;
        public string Description;

        public ComPort() { }

        public ComPort(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }


    public class GenericPort<T>
    {
        public T MyObject { get; set; }
        public T GetInstance()
        {
            T myObject;
            var type = typeof(T);
            // 類別型別，使用 Activator.CreateInstance 動態來產生物件
            if (type.Name != "String")
            {
                myObject = (T)Activator.CreateInstance(type);
            }
            else
            {
                myObject = (T)Activator.CreateInstance(type, "".ToCharArray());
            }

            return myObject;
        }
    }
}
