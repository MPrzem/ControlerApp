using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace Iotpublisher
{
    public class SensData: INotifyPropertyChanged
    {
        int soilSens;
        int rainSens;
        float airMois;
        float airTemp;
        int isOutside;
        float outVal;

        public int SoilSens { get { return soilSens; } set { soilSens = value; OnPropertyChanged(); } }
        public int RainSens { get { return rainSens; } set { rainSens = value; OnPropertyChanged(); } }
        public float AirMoiscure { get { return airMois; } set { airMois = value; OnPropertyChanged(); } }
        public float AirTemp { get { return airTemp; } set { airTemp = value; OnPropertyChanged(); } }
        public int IsOutside { get { return isOutside; } set { isOutside = value; OnPropertyChanged(); } }
        public float OutVal { get { return outVal; } set { outVal = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class Sensors
    {
        public Sensors(SensData data_)
        {
            data = data_;
        }
        public SensData data;
        public void MakeFromJson(string str)
        {
            var tmp_data = JsonSerializer.Deserialize<SensData>(str);
            data.SoilSens = tmp_data.SoilSens;
            data.RainSens = tmp_data.RainSens;
            data.OutVal = tmp_data.OutVal;
            data.IsOutside = tmp_data.IsOutside;
            data.AirTemp = tmp_data.AirTemp;
            data.AirMoiscure = tmp_data.AirMoiscure;

        }
    }
    public interface IControler
    {
        void getSensData(SensData data);
        float setOutput();
    }
}
