namespace ns3dRudderSharp
{
    public class CurveRudder : ns3DRudder.Curve
    {
        public CurveRudder(float fDeadZone, float fxSat, float fyMax, float fExp) 
            : base(fDeadZone, fxSat, fyMax, fExp)
        {
        }
    }
}
