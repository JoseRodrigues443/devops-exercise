using Microsoft.AspNetCore.Mvc;
using AgentStateAPI.Models;
using AgentStateAPI.Services;
using AgentStateAPI.Exceptions;

namespace AgentStateAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentStateController : ControllerBase
{
    private readonly IAgentStateService _agentStateService;
    private readonly ILogger<AgentStateController> _logger;

    public AgentStateController(IAgentStateService agentStateService, ILogger<AgentStateController> logger)
    {
        _agentStateService = agentStateService;
        _logger = logger;
    }

    [HttpPost("events")]
    public async Task<IActionResult> ProcessEvent([FromBody] AgentStateEvent eventData)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _agentStateService.ProcessAgentStateEventAsync(eventData);
            
            return Ok(new { message = "Agent state event processed successfully" });
        }
        catch (LateEventException ex)
        {
            _logger.LogWarning(ex, "Late event received for agent {AgentId}", eventData.AgentId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agent state event for agent {AgentId}", eventData.AgentId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}