//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Models;

namespace TeamServer.Repositories
{
    public class TaskRepository : RepositoryBase<Trinity.Shared.Models.Task>
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the TaskRepository class using the specified context.
        /// </summary>
        /// <remarks>Forwards the context to the base repository for shared initialization.</remarks>
        /// <param name="context">The context used to access the application's data store.</param>
        public TaskRepository(Context context) : base(context)
        { 
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the task with the specified ID and status name asynchronously.
        /// </summary>
        /// <param name="taskID">The task identifier.</param>
        /// <param name="status">The status name to match.</param>
        /// <returns>A Trinity.Shared.Models.Task instance matching the specified ID and status name, or null if none is found.</returns>
        public async Task<Trinity.Shared.Models.Task?> GetTaskByStatusAsync(int taskID, string status)
        {
            return await _dbSet
                .Include(t => t.Status)
                .Where(t => t.ID.Equals(taskID) && t.Status.Name.Equals(status))
                .FirstOrDefaultAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously gets queued tasks and projects them to TaskResponse instances for the specified agent.
        /// </summary>
        /// <remarks>Queries the DbSet including the Status navigation property, filters entities whose
        /// Status.Name equals "Queued", converts CommandType via Util.GetEnumValue, and constructs TaskResponse objects
        /// with the provided agentID and CreatedAt.</remarks>
        /// <param name="agentID">Agent identifier used when creating each TaskResponse.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of TaskResponse instances
        /// representing queued tasks for the specified agent.</returns>
        public async Task<List<TaskResponse>> GetQueuedAgentTasksByID(int agentID)
        {
            return await _dbSet
                .Include(t => t.Status)
                .Where(t => t.Status.Name.Equals("Queued") && t.AgentID.Equals(agentID))
                .Select(t => new TaskResponse(t.AgentID, Util.GetEnumValue(t.CommandType), t.Status.Name, t.CreatedAt))
                .ToListAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all agent tasks from the database and projects each into a TaskResponse containing agent ID,
        /// command type, status name, and creation time.
        /// </summary>
        /// <remarks>Includes the Status navigation property and converts the CommandType using
        /// Util.GetEnumValue; the query is executed asynchronously.</remarks>
        /// <returns>A Task whose result is a List<TaskResponse> containing all agent tasks.</returns>
        public async Task<List<TaskResponse>> GetAllAgentTasks()
        {
            return await _dbSet
                .Include(t => t.Status)
                .Select(t => new TaskResponse(t.AgentID, Util.GetEnumValue(t.CommandType), t.Status.Name, t.CreatedAt))
                .ToListAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves task details for the specified task ID asynchronously.
        /// </summary>
        /// <remarks>Queries the DbSet, includes the Status navigation property, projects to
        /// GetTaskDetailsResponse, and returns the first match.</remarks>
        /// <param name="taskID">Task identifier.</param>
        /// <returns>A GetTaskDetailsResponse with task details, or null if no task has the specified ID.</returns>
        public async Task<GetTaskDetailsResponse?> GetTaskDetailsByIDAsync(int taskID)
        {
            return await _dbSet
                .Include(t => t.Status)
                .Where(t => t.ID.Equals(taskID))
                .Select(t => new GetTaskDetailsResponse(t.ID, t.AgentID, Util.GetEnumValue(t.CommandType), t.Command, t.CreatedAt))
                .FirstOrDefaultAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Removes the specified task from the context's entity set.
        /// </summary>
        /// <remarks>Marks the entity for deletion in the context; call SaveChanges to persist the
        /// removal.</remarks>
        /// <param name="task">The task to remove.</param>
        public void RemoveTask(Trinity.Shared.Models.Task task)
        {
            _dbSet.Remove(task);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Removes all queued tasks for the specified agent from the database asynchronously.
        /// </summary>
        /// <remarks>Deletes tasks with AgentID equal to the specified value and Status.Name equal to
        /// "Queued" using a server-side bulk delete (ExecuteDeleteAsync).</remarks>
        /// <param name="agentID">Identifier of the agent whose queued tasks will be removed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async System.Threading.Tasks.Task ClearTasksByAgentID(int agentID)
        {
            await _dbSet
                .Include(t => t.Status)
                .Where(t => t.AgentID.Equals(agentID) && t.Status.Name.Equals("Queued"))
                .ExecuteDeleteAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves the first queued task for the specified agent, or null if none exists.
        /// </summary>
        /// <remarks>Includes the Status navigation property and filters tasks where Status.Name equals
        /// "Queued".</remarks>
        /// <param name="agentID">Identifier of the agent to find the queued task for.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the first queued Task for the
        /// specified agent, or null if none is found.</returns>
        public async Task<Trinity.Shared.Models.Task?> GetTaskFromQueueAsync(int agentID) 
        {
            return await _dbSet
                .Include(t => t.Status)
                .Where(t => t.AgentID.Equals(agentID) && t.Status.Name.Equals("Queued"))
                .FirstOrDefaultAsync();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//