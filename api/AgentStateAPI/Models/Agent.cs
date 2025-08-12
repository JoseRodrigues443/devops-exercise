using System.ComponentModel.DataAnnotations;

namespace AgentStateAPI.Models;

public class Agent
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public AgentState State { get; set; }
    
    public DateTime LastUpdatedUtc { get; set; }
    
    public virtual ICollection<AgentSkill> Skills { get; set; } = new List<AgentSkill>();
}

public class AgentSkill
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public Guid AgentId { get; set; }
    
    [Required]
    public Guid QueueId { get; set; }
    
    public virtual Agent Agent { get; set; } = null!;
}