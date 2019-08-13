namespace Capstones.Net
{
    public static class ProtocolExtensions
    {
        public static bool IsPitcherDominateActive(this Protocols.DominateStatus dominate)
        {
            if (dominate != null)
            {
                for (int i = (int)Protocols.DominateType.PitcherDominate1; i < dominate.ActiveOpStatus.Count; ++i)
                {
                    if (dominate.ActiveOpStatus[i].IsDominateActive())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool IsBatterDominateActive(this Protocols.DominateStatus dominate)
        {
            if (dominate != null)
            {
                for (int i = (int)Protocols.DominateType.BatterDominate1; i < dominate.ActiveOpStatus.Count && i < (int)Protocols.DominateType.PitcherDominate1; ++i)
                {
                    if (dominate.ActiveOpStatus[i].IsDominateActive())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsDominateActive(this Protocols.DominateOpStatus dominate)
        {
            if (dominate == null)
            {
                return false;
            }
            if (dominate.Count > 0)
            {
                return true;
            }
            //for (int i = 0; i < dominate.CountByPitchType.Count; ++i)
            //{
            //    if (dominate.CountByPitchType[i] > 0)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }
    }
}
