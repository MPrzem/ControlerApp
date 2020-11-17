using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using System.IO;
using Path = System.IO.Path;
using Iotpublisher;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SensData sensData=new SensData();
        PID_Values Pidval = new PID_Values();
        HiLevComm com;
        public MainWindow()
        {
            InitializeComponent();
            string iotEndpoint = "aaquc5o1pax6f-ats.iot.eu-north-1.amazonaws.com";
            var caCert = X509Certificate.CreateFromCertFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AmazonRootCA1.crt"));
            var clientCert = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.cert.pfx"), "ZAQ!2wsxcde3");
            var client = new MqttClient(iotEndpoint, 8883, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Pidval.HUM_expected = 2900;
            Pidval.KP = (float)1.1;
            Pidval.TD = 0;
            Pidval.TI = 180;
            Hum_exp.Text = Pidval.HUM_expected.ToString();
            Kp.Text = Pidval.KP.ToString();
            Td.Text = Pidval.TD.ToString();
            Ti.Text = Pidval.TI.ToString();
            var PidControler = new PID( 10,Pidval, new Iotpublisher.Integral(10, 800));
            com = new HiLevComm(client,PidControler,sensData);
            DataContext = sensData;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lock (com.SyncObject)
            {
                Pidval.HUM_expected = float.Parse(Hum_exp.Text);
                Pidval.KP = float.Parse(Kp.Text);
                Pidval.TD = Int32.Parse(Td.Text);
                Pidval.TI = float.Parse(Ti.Text);
            }
        }
    }
}
