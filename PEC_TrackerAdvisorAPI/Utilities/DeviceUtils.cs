using PEC_TrackerAdvisorAPI.Enums;

namespace PEC_TrackerAdvisorAPI.Utilities
{
    public static class DeviceUtils
    {
        public static DeviceUsage GetDeviceUsage(int hoursUsed)
        {
            if(hoursUsed < 2)
            {
                return DeviceUsage.Low;
            }
            else if(hoursUsed >= 2 && hoursUsed < 4)
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
