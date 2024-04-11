using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEC_TrackerAdvisorAPI.Context;
using PEC_TrackerAdvisorAPI.Models;
using PEC_TrackerAdvisorAPI.Utilities;

namespace PEC_TrackerAdvisorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _authContext;

        public DeviceController(AppDbContext authContext)
        {
            _authContext = authContext;
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddDeviceAsync(Device device)
        {
            try
            {
                device.Usage = DeviceUtils.GetDeviceUsage(device.HoursUsed);
                await _authContext.Devices.AddAsync(device);
                await _authContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetDevicesAsync([FromQuery] long userId)
        {
            try
            {
                var devices = await _authContext.Devices.Where(d => d.UserId == userId).ToListAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
