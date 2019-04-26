using ns3DRudder;
using System;

using NSCurve = ns3DRudder.Curve;

namespace ns3dRudderSharp
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public sealed class RudderController
    {
        private Axis m_Axis;
        private Rudder m_Rudder;
        private bool m_UseCurve;
        private CurveArray m_CurveArray;
        private NSCurve m_CurveRoll;
        private NSCurve m_CurvePitch;
        private NSCurve m_CurveUpDown;
        private NSCurve m_CurveYaw;
        private ModeAxis m_ModeAxis;
        private bool m_Available;

        public bool Connected { get; private set; }
        public bool Available => m_Available;

        public event Action<bool> ConnectionChanged = null;

        public RudderController(bool useCurve = true)
        {
            m_Available = true;
            m_UseCurve = useCurve;

            try
            {
                var manager = new RudderManager();
                m_Rudder = manager.GetRudder(0);
            }
            catch (Exception ex)
            {
                m_Available = false;
                Console.WriteLine(ex.Message);
                return;
            }

            m_Axis = new Axis();
            m_CurveArray = new CurveArray();
            m_ModeAxis = m_UseCurve ? ModeAxis.ValueWithCurveNonSymmetricalPitch : ModeAxis.NormalizedValue;

            //-- Sensivity (angle Speed Max) 
            //-- Value normalized [0,1]
            var roll = 0.6f;  // 60%
            var pitch = 0.4f;
            var yaw = 0.8f;
            var upDown = 0.4f;

            //-- Death zone (%)
            var deathzoneRoll = 0.25f * roll; // 10 %
            var deathzonePitch = 0.25f * pitch;
            var deathzoneYaw = 0.1f * yaw;
            var deathzoneUpDown = 0.3f * upDown;

            //-- Define  curves
            m_CurveRoll = new NSCurve(deathzoneRoll, roll, 1.0f, 1.0f);
            m_CurvePitch = new NSCurve(deathzonePitch, pitch, 1.0f, 1.0f);
            m_CurveYaw = new NSCurve(deathzoneYaw, yaw, 1.0f, 1.0f);
            m_CurveUpDown = new NSCurve(deathzoneUpDown, upDown, 1.0f, 2.0f);

            m_CurveArray.SetCurve(CurveType.CurvePitch, m_CurvePitch);
            m_CurveArray.SetCurve(CurveType.CurveRoll, m_CurveRoll);
            m_CurveArray.SetCurve(CurveType.CurveYaw, m_CurveYaw);
            m_CurveArray.SetCurve(CurveType.CurveUpDown, m_CurveUpDown);
        }

        public bool GetAxis(ref float x, ref float y, ref float z, ref float ry)
        {
            if (!m_Available)
                return false;

            if (!Connected)
            {
                if (m_Rudder.IsDeviceConnected())
                {
                    Connected = true;
                    ConnectionChanged?.Invoke(true);
                }
            }

            if (!Connected)
                return false;

            if (m_UseCurve)
                m_Axis = m_Rudder.GetAxisWithCurve(m_ModeAxis, m_CurveArray);
            else
                m_Axis = m_Rudder.GetAxis(m_ModeAxis);

            x = -m_Axis.GetXAxis();
            y = m_Axis.GetZAxis();
            z = m_Axis.GetYAxis();
            ry += -m_Axis.GetZRotation();

            return true;
        }

        public void UpdateTransform(ref Vector3 translation, ref Vector3 rotation, float moveSpeed, float rotationSpeed, bool updateUpTranslation)
        {
            if (!m_Available)
                return;

            if (!Connected)
            {
                if (m_Rudder.IsDeviceConnected())
                {
                    Connected = true;
                    ConnectionChanged?.Invoke(true);
                }
            }

            if (!Connected)
                return;

            if (m_UseCurve)
                m_Axis = m_Rudder.GetAxisWithCurve(m_ModeAxis, m_CurveArray);
            else
                m_Axis = m_Rudder.GetAxis(m_ModeAxis);

            translation.X += -m_Axis.GetXAxis() * moveSpeed;
            translation.Z += m_Axis.GetYAxis() * moveSpeed;

            if (updateUpTranslation)
                translation.Y += m_Axis.GetZAxis();

            rotation.Y += -m_Axis.GetZRotation() * rotationSpeed;
        }
    }
}