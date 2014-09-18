using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CSettings
    {
        public string SerialPortName { get; set; }

        public CSettings()
        {
            this.SerialPortName = "COM7";
        }

        public void LoadFromFile(string path)
        {
            System.IO.StreamReader fileReader = new System.IO.StreamReader(path);
            string[] settingsFileLines = fileReader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            this.SerialPortName = settingsFileLines[1];

            fileReader.Close();
        }
    }
}
