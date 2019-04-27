using ns3dRudder;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private double Y = 0;
        private double Z = 0;
        private double Yaw = 0;
        private bool m_IsRunning;
        private bool m_RotationEnabled = false;
        private bool m_SimulateCrouch = true;
        private float m_MoveSpeed = 0.25f;
        private float m_RotationSpeed = 0.25f;
        private bool m_RudderEnabled = true;
        private float m_HeadHeight = 1.8f;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.3dRudder", Capabilities.HeadTracking);
            m_AxesParamDefault = new AxesParamDefault();
            m_AxesValue = new AxesValue();

            InitializeComponent();

            SetTextValue(HeadHeight, m_HeadHeight);
            SetTextValue(MoveSpeed, m_MoveSpeed);
            SetTextValue(RotationSpeed, m_RotationSpeed);
            SetToggleValue(RotationEnabled, m_RotationEnabled);
            SetToggleValue(RudderEnabled, m_RudderEnabled);
            SetToggleValue(SimulateCrouch, m_SimulateCrouch);
        }

        private void SetTextValue(TextBox textBox, object value) => textBox.Text = value.ToString();
        private void SetToggleValue(CheckBox box, bool value) => box.IsChecked = value;

        public void Start()
        {
            Stop();

            var status = Sdk3dRudder.Init();

            m_IsRunning = status != ErrorCode.Fail;

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

        private void Recenter()
        {
            X = 0;
            Y = m_HeadHeight;
            Z = 0;
            Yaw = 0;
        }

        private void ThreadLoop()
        {
            Y = m_HeadHeight;

            while (m_IsRunning)
            {
                if (m_RudderEnabled)
                {
                    var errorCode = Sdk3dRudder.GetAxes(0, m_AxesParamDefault, m_AxesValue);

                    if (errorCode == ErrorCode.Success)
                    {
                        X += m_AxesValue.Get(Axes.LeftRight) * m_MoveSpeed;
                        Z -= m_AxesValue.Get(Axes.ForwardBackward) * m_MoveSpeed;

                        if (m_SimulateCrouch)
                        {
                            var y = m_AxesValue.Get(Axes.UpDown);
                            if (y < -0.5f)
                                Y = m_HeadHeight / 2;
                            else
                                Y = m_HeadHeight;
                        }

#if DEBUG
                        Console.WriteLine($"X: {X}, Y: {Y}, Z: {Z}");
#endif

                        if (m_VridgeRemote.Head != null)
                        {
                            if (m_RotationEnabled)
                            {
                                Yaw += m_AxesValue.Get(Axes.Rotation) * m_RotationSpeed;
                                m_VridgeRemote.Head.SetRotationAndPosition(Yaw, 0, 0, X, Y, Z);
                            }
                            else
                                m_VridgeRemote.Head.SetPosition(X, Y, Z);
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void OnConnectClicked(object sender, RoutedEventArgs e) => Start();
        private void OnDisconnectClicked(object sender, RoutedEventArgs e) => Stop();
        private void OnRecenterClicked(object sender, RoutedEventArgs e) => Recenter();

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
            Application.Current.Shutdown();
        }

        private void HandleCheckbox(object sender, ref bool target)
        {
            var checkbox = (CheckBox)sender;

            if (checkbox.IsChecked.HasValue)
                target = checkbox.IsChecked.Value;
            else
                target = false;
        }

        private void HandleTextBoxFloat(object sender, ref float target)
        {
            var text = (TextBox)sender;

            if (float.TryParse(text.Text, out float result))
                target = result;
        }

        private void RudderEnabled_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_RudderEnabled);
        }

        private void RotationEnabled_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_RotationEnabled);
        }

        private void SimulateCrouch_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_SimulateCrouch);
        }

        private void HeadHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextBoxFloat(sender, ref m_HeadHeight);
        }

        private void MoveSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextBoxFloat(sender, ref m_MoveSpeed);
        }

        private void RotationSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextBoxFloat(sender, ref m_RotationSpeed);
        }
    }
}
