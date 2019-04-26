using ns3dRudderSharp;
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
        private RudderController m_3dRudder;
        private bool m_IsRunning;

        public bool RudderEnabled { get; set; } = true;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.3dRudder", Capabilities.HeadTracking);
            m_3dRudder = new RudderController();

            InitializeComponent();

            DataContext = this;
        }

        public void Start()
        {
            Stop();

            m_IsRunning = true;

            m_Thread = new Thread(new ThreadStart(ThreadLoop));
            m_Thread.Start();

            StatusLabel.Text = "Started";
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
            var x = 0.0f;
            var y = 0.0f;
            var z = 0.0f;
            var ry = 0.0f;

            while (m_IsRunning)
            {
                if (RudderEnabled)
                {
                    m_3dRudder.GetAxis(ref x, ref y, ref z, ref ry);

                    if (m_VridgeRemote.Head != null)
                        m_VridgeRemote.Head.SetPosition((double)x, 0, (double)z);

                    Console.WriteLine($"x: {x}, y: {y}, z: {z}, rY: {ry}");
                }

                Thread.Sleep(10);
            }
        }

        private void OnConnectClicked(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void OnDisconnectClicked(object sender, RoutedEventArgs e)
        {
            Stop();
        }
    }
}
