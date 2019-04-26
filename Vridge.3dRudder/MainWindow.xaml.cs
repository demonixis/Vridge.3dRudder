using ns3dRudderSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            m_IsRunning = true;

            InitializeComponent();

            DataContext = this;
        }

        public void Start()
        {
            Stop();

            m_Thread = new Thread(new ThreadStart(ThreadLoop));
            m_Thread.Start();
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
                    m_VridgeRemote.Head.SetPosition((double)x, (double)y, (double)z);
                }

                Thread.Sleep(10);
            }
        }
    }
}
