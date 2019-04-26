using ns3DRudder;
using System;

namespace ns3dRudderSharp
{
    public class Rudder : IDisposable
    {
        private uint m_PortNumber;
        private Axis m_Axis;
        private Axis m_AxisOffset;
        private RudderManager m_SDK;

        public bool Freezed { get; private set; }

        public Rudder(uint portNumber, RudderManager sdk)
        {
            m_PortNumber = portNumber;
            m_SDK = sdk;
            m_Axis = new Axis();
            m_AxisOffset = new Axis();
            Freezed = false;
        }

        public void Dispose()
        {
            m_Axis.Dispose();
            m_AxisOffset.Dispose();
        }

        ~Rudder() => Dispose();

        public bool IsDeviceConnected() => m_SDK.IsDeviceConnected(m_PortNumber);

        public uint GetVersion() => m_SDK.GetVersion(m_PortNumber);

        public bool IsSystemDeviceHidden() => m_SDK.IsSystemDeviceHidden(m_PortNumber);

        public Status GetStatus() => m_SDK.GetStatus(m_PortNumber);

        public uint GetSensor(uint index) => m_SDK.GetSensor(m_PortNumber, index);

        public void HideSystemDevice(bool hide)
        {
            var error = m_SDK.HideSystemDevice(m_PortNumber, hide);

            if (error != ErrorCode.Success)
                Console.WriteLine("HideSystemDevice : {0}, portnumber : {1}, hide : {2}", error, m_PortNumber, hide);
        }

        public void PlaySnd(ushort frequencyHz, ushort durationMillisecond)
        {
            var error = m_SDK.PlaySnd(m_PortNumber, frequencyHz, durationMillisecond);

            if (error != ErrorCode.Success)
                Console.WriteLine("PlaySnd : {0}, frequency : {1}, duration : {2}", error, frequencyHz, durationMillisecond);
        }

        public void SetFreeze(bool active)
        {
            Freezed = active;
            var error = m_SDK.SetFreeze(m_PortNumber, Freezed);

            if (error != ErrorCode.Success)
                Console.WriteLine("Freeze : {0}, active : {1}", error, Freezed);
        }

        public Axis GetUserOffset()
        {
            var error = m_SDK.GetUserOffset(m_PortNumber, m_AxisOffset);

            if (error != ErrorCode.Success)
                Console.WriteLine("GetUserOffset : {0}, portnumber : {1}", error, m_PortNumber);

            return m_AxisOffset;
        }

        public Axis GetAxis(ModeAxis mode = ModeAxis.NormalizedValue)
        {
            var error = m_SDK.GetAxis(m_PortNumber, mode, m_Axis);

            if (error != ErrorCode.Success)
                Console.WriteLine("GetAxis : {0}, portnumber : {1}, mode : {2}", error, m_PortNumber, mode);

            return m_Axis;
        }

        public Axis GetAxisWithCurve(ModeAxis mode, CurveArray curves)
        {
            var error = m_SDK.GetAxis(m_PortNumber, mode, m_Axis, curves);

            if (error != ErrorCode.Success)
                Console.WriteLine("GetAxisWithCurve : {0}, portnumber : {1}, mode : {2}", error, m_PortNumber, mode);

            return m_Axis;
        }

        public void GetAxis3D(Axis pAxis, ref float x, ref float y, ref float z)
        {
            if (pAxis != null)
            {
                x = pAxis.GetXAxis();
                y = pAxis.GetZAxis();
                z = pAxis.GetYAxis();
            }

            GetAxis();

            x = m_Axis.GetXAxis();
            y = m_Axis.GetZAxis();
            z = m_Axis.GetYAxis();
        }
    }
}