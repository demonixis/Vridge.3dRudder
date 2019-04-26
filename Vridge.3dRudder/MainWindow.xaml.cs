using ns3dRudder;
using System;
using System.Threading;
using System.Windows;
using VRE.Vridge.API.Client.Remotes;

namespace Vridge._3dRudder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class AxesParam : IAxesParam
        {
        }

        private Thread m_Thread;
        private VridgeRemote m_VridgeRemote;
        private IAxesParam m_IAxesParam;
        private AxesValue m_AxesValue;
        private double X = 0;
        private double Z = 0;

        private bool m_IsRunning;

        public bool RudderEnabled { get; set; } = true;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.3dRudder", Capabilities.HeadTracking);
            m_IAxesParam = new AxesParamDefault();
            m_AxesValue = new AxesValue();

            InitializeComponent();

            DataContext = this;
        }

        public void Start()
        {
            Stop();

            var status = Sdk3dRudder.Init();

            m_IsRunning = status == ErrorCode.Success;

            if (m_IsRunning)
            {
                m_Thread = new Thread(new ThreadStart(ThreadLoop));
                m_Thread.Start();

                StatusLabel.Text = "Started";
            }
            else
                StatusLabel.Text = status.ToString();
        }

        public void Stop()
        {
            m_IsRunning = false;

            if (m_Thread != null)
            {
                if (m_Thread.IsAlive)
                    m_Thread.Abort();

                m_Thread = null;
            }

            StatusLabel.Text = "Stopped";
        }

        private void ThreadLoop()
        {
            while (m_IsRunning)
            {
                if (RudderEnabled)
                {
                    if (m_VridgeRemote.Head != null)
                    {
                        Sdk3dRudder.GetAxes(0, m_IAxesParam, m_AxesValue);

                        Z += m_AxesValue.Get(Axes.ForwardBackward);
                        X += m_AxesValue.Get(Axes.LeftRight);

                        m_VridgeRemote.Head.SetPosition(X, 0, Z);
                        Console.WriteLine($"x: {X}, z: {Z}");
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void OnConnectClicked(object sender, RoutedEventArgs e) => Start();
        private void OnDisconnectClicked(object sender, RoutedEventArgs e) => Stop();
    }
}
