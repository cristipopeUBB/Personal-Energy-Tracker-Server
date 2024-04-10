using Microsoft.AspNetCore.Mvc;
using PEC_TrackerAdvisorAPI.Context;
using PEC_TrackerAdvisorAPI.Models;

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
        public async Task<IActionResult> AddDevice(Device device)
        {
            try
            {
                await _authContext.Devices.AddAsync(device);
                await _authContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
