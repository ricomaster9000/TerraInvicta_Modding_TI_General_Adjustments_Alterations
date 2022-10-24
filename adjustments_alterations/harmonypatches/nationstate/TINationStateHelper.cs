using UnityEngine;

namespace TI_General_Adjustments_Alterations.adjustment_alterations.core.nationstate
{
    public class TINationStateHelper
    {
        public static int getNumControlPointsUnclamped(double GDP)
        {
            int getNumControlPoints_unclampedMethodResult = Mathf.Max(Mathf.RoundToInt((float)Mathd.Pow(GDP / 1000000000.0/*per billion*/, 0.25) / 2f), 1);
            return Mathf.Min(getNumControlPoints_unclampedMethodResult, 6);
        }
    }
}