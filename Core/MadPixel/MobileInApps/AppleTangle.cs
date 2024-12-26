using System;

namespace MadPixel.InApps
{
    public static class AppleTangle
    {
        private static Func<byte[]> _getterData; 
        
        public static void SetData(Func<byte[]> getterData)
        {
            _getterData = getterData;
        }
        public static byte[] Data()
        {
            return _getterData.Invoke();
        }
    }
}