using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TeamServer.Controllers
{
    [Route(Routes.Tasks)]
    [ApiController]
    public class TaskController : ControllerBase
    {

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TaskController()
        {
            
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all tasks.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/tasks")]
        public async Task<IActionResult> GetTasks() 
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves a task by ID.
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        [HttpGet("/tasks/{taskID")]
        public async Task<IActionResult> GetTaskByID([FromRoute] int taskID)
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves tasks associated to a specified agent.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet("/{agentID}")]
        public async Task<IActionResult> GetTasksByAgentID([FromRoute] int agentID)
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all active downloads associated to a specified agent.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet("/{agentID}/activeDownloads")]
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
        public async Task<IActionResult> ClearQueue([FromRoute] int taskID) 
        {
            return Ok();
        }
    }
}
