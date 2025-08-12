using AgentStateAPI.Data;
using AgentStateAPI.Exceptions;
using AgentStateAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentStateAPI.Services;

public interface IAgentStateService
{
    Task ProcessAgentStateEventAsync(AgentStateEvent eventData);
}

public class AgentStateService : IAgentStateService
{
    private readonly AgentStateDbContext _context;
    private readonly ILogger<AgentStateService> _logger;

    public AgentStateService(AgentStateDbContext context, ILogger<AgentStateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessAgentStateEventAsync(AgentStateEvent eventData)
    {
        // Validate timestamp - throw exception if more than an hour old
        var hourAgo = DateTime.UtcNow.AddHours(-1);
        if (eventData.TimestampUtc < hourAgo)
        {
            throw new LateEventException($"Event timestamp {eventData.TimestampUtc} is more than an hour old");
        }

        // Calculate agent state based on business rules
        var newState = CalculateAgentState(eventData);

        // Find or create agent
        var agent = await _context.Agents
            .Include(a => a.Skills)
            .FirstOrDefaultAsync(a => a.Id == eventData.AgentId);

        if (agent == null)
        {
            agent = new Agent
            {
                Id = eventData.AgentId,
                Name = eventData.AgentName,
                State = newState,
                LastUpdatedUtc = eventData.TimestampUtc
            };
            _context.Agents.Add(agent);
        }
        else
        {
            agent.Name = eventData.AgentName;
            agent.State = newState;
            agent.LastUpdatedUtc = eventData.TimestampUtc;
        }

        // Synchronize skills with queueIds
        await SynchronizeAgentSkillsAsync(agent, eventData.QueueIds);

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Processed agent state event for agent {AgentId}, new state: {State}", 
            eventData.AgentId, newState);
    }

    private static AgentState CalculateAgentState(AgentStateEvent eventData)
    {
        return eventData.Action switch
        {
            "START_DO_NOT_DISTURB" when IsLunchTime(eventData.TimestampUtc) => AgentState.ON_LUNCH,
            "CALL_STARTED" => AgentState.ON_CALL,
            "START_DO_NOT_DISTURB" => AgentState.DO_NOT_DISTURB,
            _ => AgentState.AVAILABLE
        };
    }

    private static bool IsLunchTime(DateTime timestampUtc)
    {
        var hour = timestampUtc.Hour;
        return hour >= 11 && hour < 13; // 11AM to 1PM UTC
    }

    private async Task SynchronizeAgentSkillsAsync(Agent agent, List<Guid> queueIds)
    {
        // Remove skills not in the new queue list
        var existingSkills = agent.Skills.ToList();
        var skillsToRemove = existingSkills.Where(s => !queueIds.Contains(s.QueueId)).ToList();
        
        foreach (var skill in skillsToRemove)
        {
            _context.AgentSkills.Remove(skill);
        }

        // Add new skills
        var existingQueueIds = existingSkills.Select(s => s.QueueId).ToHashSet();
        var newQueueIds = queueIds.Where(qId => !existingQueueIds.Contains(qId));

        foreach (var queueId in newQueueIds)
        {
            agent.Skills.Add(new AgentSkill
            {
                AgentId = agent.Id,
                QueueId = queueId
            });
        }
    }
}