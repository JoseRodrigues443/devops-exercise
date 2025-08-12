using System.ComponentModel.DataAnnotations;

namespace AgentStateAPI.Models;

public class AgentStateEvent
{
    [Required]
    public Guid AgentId { get; set; }
    
    [Required]
    public string AgentName { get; set; } = string.Empty;
    
    [Required]
    public DateTime TimestampUtc { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    public List<Guid> QueueIds { get; set; } = new();
}

public enum AgentState
{
    AVAILABLE,
    ON_CALL,
    ON_LUNCH,
    DO_NOT_DISTURB
}