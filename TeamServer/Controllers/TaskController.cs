//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.DTOs.Tasks;

namespace TeamServer.Controllers
{
    [Route(Routes.Tasks)]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private TaskService _taskService;
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TaskController(TaskService taskService)
        {
            this._taskService = taskService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all tasks.
        /// </summary>
        /// <returns></returns>
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks() 
        {
            List<TaskDTO> response = await _taskService.GetTasksAsync();
            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves tasks associated to a specified agent.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet("{agentID}")]
        public async Task<IActionResult> GetTasksByAgentID([FromRoute] int agentID)
        {
            List<TaskDTO> response = await _taskService.GetTasksByAgentAsync(agentID);
            return Ok(response);
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
        /// De-queues a task with a pending state.
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        [HttpGet("/{taskID}/stop")]
        public async Task<IActionResult> StopTask([FromRoute] int taskID)
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// De-queues all tasks in the agent queue. 
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        [HttpDelete("/{agentID}/clearQueue")]
        public async Task<IActionResult> ClearQueue([FromRoute] int agentID) 
        {
            var result = await _taskService.ClearAgentTasksAsync(agentID);
            if (!result)
            {
                return BadRequest();    
            }

            return Ok();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//