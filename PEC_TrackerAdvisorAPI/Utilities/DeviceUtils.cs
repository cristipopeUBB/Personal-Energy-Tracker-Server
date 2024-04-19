using PEC_TrackerAdvisorAPI.Enums;

namespace PEC_TrackerAdvisorAPI.Utilities
{
    public static class DeviceUtils
    {
        public static DeviceUsage GetDeviceUsage(int hoursUsed)
        {
            if(hoursUsed < 3)
            {
                return DeviceUsage.Low;
            }
            else if(hoursUsed >= 3 && hoursUsed < 6)
            {
                return DeviceUsage.Medium;
            }
            else
            {
                return DeviceUsage.High;
            }
        }
    }
}
