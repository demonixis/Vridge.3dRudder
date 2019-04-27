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
        private Thread m_Thread;
        private VridgeRemote m_VridgeRemote;
        private AxesParamDefault m_AxesParamDefault;
        private AxesValue m_AxesValue;
        private double X = 0;
        private double Y = 1.8;
        private double Z = 0;

        private bool m_IsRunning;

        public float MoveSpeed { get; set; } = 0.25f;
        public bool RudderEnabled { get; set; } = true;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.3dRudder", Capabilities.HeadTracking);
            m_AxesParamDefault = new AxesParamDefault();
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
                    var errorCode = Sdk3dRudder.GetAxes(0, m_AxesParamDefault, m_AxesValue);

                    if (errorCode == ErrorCode.Success)
                    {
                        X += m_AxesValue.Get(Axes.LeftRight) * MoveSpeed;
                        Z -= m_AxesValue.Get(Axes.ForwardBackward) * MoveSpeed;

                        if (m_VridgeRemote.Head != null)
                            m_VridgeRemote.Head.SetPosition(X, Y, Z);
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void OnConnectClicked(object sender, RoutedEventArgs e) => Start();
        private void OnDisconnectClicked(object sender, RoutedEventArgs e) => Stop();
    }
}
