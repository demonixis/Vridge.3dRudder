using ns3DRudder;
using System;

namespace ns3dRudderSharp
{
    public class RudderManager : CSdk
    {
        private const int ThreadSleepMs = 100;
        public static readonly int SDKMaxDevice = i3DR._3DRUDDER_SDK_MAX_DEVICE;
        public static readonly int SDKVersion = i3DR._3DRUDDER_SDK_VERSION;
        private static RudderManager s_Instance;
        private Rudder[] m_Rudders;

        public static RudderManager Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new RudderManager();

                return s_Instance;
            }
        }

        private RudderManager()
        {
            Init();

            m_Rudders = new Rudder[SDKMaxDevice];

            for (uint i = 0; i < m_Rudders.Length; ++i)
                m_Rudders[i] = new Rudder(i, this);

            Console.WriteLine("SDK version : {0:X4}", GetSDKVersion());
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (Rudder rudder in m_Rudders)
                rudder.Dispose();

            s_Instance = null;

            GC.SuppressFinalize(this);
        }

        public Rudder GetRudder(int portNumber) => m_Rudders[portNumber];
    }
}