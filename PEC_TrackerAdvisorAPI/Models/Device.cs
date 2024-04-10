using PEC_TrackerAdvisorAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace PEC_TrackerAdvisorAPI.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } // To whom the device is assigned
        public string? Name { get; set; }
        public int Consumption { get; set; }
        public DeviceUsage Usage { get; set; }
    }
}
