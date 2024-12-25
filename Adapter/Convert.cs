using System;
using LittleBitGames.Environment.Ads;
using MAXHelper;

namespace LittleBit.MPC.Adapter
{
    public class Convert
    {
        public class Ad
        {
            public static AdsManager.EAdType ToMadPixel(AdType adType)
            {
                switch (adType)
                {
                    case AdType.Inter: return AdsManager.EAdType.INTER;
                    case AdType.Banner: return AdsManager.EAdType.BANNER;
                    case AdType.Rewarded: return AdsManager.EAdType.REWARDED;
                }

                throw new NotImplementedException();
            }

            public static AdType ToLittleBit(AdsManager.EAdType adType)
            {
                switch (adType)
                {
                    case AdsManager.EAdType.INTER: return AdType.Inter;
                    case AdsManager.EAdType.BANNER: return AdType.Banner;
                    case AdsManager.EAdType.REWARDED: return AdType.Rewarded;
                }

                throw new NotImplementedException();
            }
        }
    }
}
