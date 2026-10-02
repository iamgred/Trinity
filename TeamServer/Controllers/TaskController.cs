//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;


namespace TeamServer.Controllers
{
    [Route(Routes.Tasks)]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks() 
        {
            var result = await _taskService.GetAllAgentTasks();
            return Ok(result.Response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the details of the task with the specified identifier.
        /// </summary>
        /// <remarks>Handles HTTP GET requests at 'tasks/{ID}'.</remarks>
        /// <param name="ID">The identifier of the task to retrieve.</param>
        /// <returns>An IActionResult that returns 200 (OK) with the task details when found, or 404 (NotFound) when no task
        /// exists with the specified ID.</returns>
        [HttpGet("tasks/{ID}")]
        public async Task<IActionResult> GetTaskDetailsByID([FromRoute] int ID)
        {
            var result = await _taskService.GetTaskDetailsByIDAsync(ID);
            return result.IsSuccess ? Ok(result.Response) : NotFound(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves queued tasks for the specified agent.
        /// </summary>
        /// <remarks>Asynchronously calls the task service to obtain the agent's queued tasks.</remarks>
        /// <param name="agentID">Identifier of the agent whose queued tasks are retrieved.</param>
        /// <returns>An IActionResult that returns 200 (OK) with the queued tasks on success, or 400 (BadRequest) with error
        /// details on failure.</returns>
        [HttpGet("{agentID}")]
        public async Task<IActionResult> GetTasksByAgentID([FromRoute] int agentID)
        {
            var result = await _taskService.GetQueuedTasksByAgentID(agentID);
            return result.IsSuccess ? Ok(result.Response) : NotFound(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all active downloads associated to a specified agent.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet("{agentID}/activeDownloads")]
        public async Task<IActionResult> GetActiveDownloadsByAgentID([FromRoute] int agentID)
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stops the task with the specified identifier.
        /// </summary>
        /// <remarks>Mapped to HTTP GET /{taskID}/stop. Invokes the task service asynchronously to remove
        /// the task.</remarks>
        /// <param name="taskID">The identifier of the task to stop.</param>
        /// <returns>200 (OK) when the task was removed; 404 (NotFound) with an error if no task with the specified ID exists.</returns>
        [HttpDelete("/{taskID}/stop")]
        public async Task<IActionResult> StopTask([FromRoute] int taskID)
        {
            var result = await _taskService.RemoveTaskByIDAsync(taskID);
            return result.IsSuccess ? Ok() : NotFound(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Clears all queued tasks for the agent identified by agentID.
        /// </summary>
        /// <remarks>Accessible via HTTP DELETE at /{agentID}/clearQueue.</remarks>
        /// <param name="agentID">Identifier of the agent whose queued tasks to clear.</param>
        /// <returns>An IActionResult: 200 (OK) when the queue is cleared; 400 (Bad Request) with an error on failure.</returns>
        [HttpDelete("/{agentID}/clearQueue")]
        public async Task<IActionResult> ClearQueue([FromRoute] int agentID) 
        {
            var result = await _taskService.ClearAgentTasksAsync(agentID);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//