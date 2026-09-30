//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //    
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TeamServer.DTOs.Listeners;
using TeamServer.Modules;
using TeamServer.Services;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Controllers
{
    [Route(Routes.Listeners)]
    [ApiController]
    public class ListenerController : ControllerBase
    {
        private ListenerService _listenerService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of ListenerController with the specified ListenerService.
        /// </summary>
        /// <remarks>Intended for use with dependency injection.</remarks>
        /// <param name="listenerService">The ListenerService used by the controller to perform listener-related operations.</param>
        public ListenerController(ListenerService listenerService)
        {
            _listenerService = listenerService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets all HTTP listeners.
        /// </summary>
        /// <remarks>Retrieves listeners asynchronously from the configured listener service.</remarks>
        /// <returns>An IActionResult that produces an HTTP 200 (OK) response containing the collection of listeners.</returns>
        [HttpGet]
        public async Task<IActionResult> GetHttpListeners()
        {
            var response = await _listenerService.GetListenersAsync();
            return Ok(response);
        }

        [HttpPost("http/restart/{ID}")]
        public async Task<IActionResult> RestartListenerAsync([FromRoute] int ID)
        {
            return Ok();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates and starts an HTTP listener using the specified request.
        /// </summary>
        /// <remarks>Performs the operation asynchronously and delegates creation to the listener
        /// service.</remarks>
        /// <param name="request">The request that specifies configuration for the HTTP listener to create.</param>
        /// <returns>An IActionResult that is 201 Created when the listener is created; otherwise 400 Bad Request with an error.</returns>
        [HttpPost("http")]
        public async Task<IActionResult> StartHttpListener([FromBody] CreateHttpListenerRequest request)
        {
            var result = await _listenerService.CreateHttpListenerAsync(request);
            return result.IsSuccess ? Created() : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a TCP listener from the provided request and returns an HTTP response indicating success or failure.
        /// </summary>
        /// <remarks>Delegates listener creation to the listener service and awaits the asynchronous
        /// result.</remarks>
        /// <param name="request">Configuration and parameters for the TCP listener supplied in the request body.</param>
        /// <returns>An IActionResult: 201 Created on success; 400 Bad Request with error details on failure.</returns>
        [HttpPost("tcp")]
        public async Task<IActionResult> CreateTcpListenerAsync([FromBody] CreateTcpListenerRequest request)
        {
            var result = await _listenerService.CreateTCPListenerAsync(request);
            return result.IsSuccess ? Created() : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a new SMB listener from the specified request.
        /// </summary>
        /// <remarks>Delegates creation to the listener service and returns appropriate HTTP status
        /// codes.</remarks>
        /// <param name="request">Details for the SMB listener to create.</param>
        /// <returns>An IActionResult that returns 201 Created when the listener is created successfully, or 400 Bad Request with
        /// an error when creation fails.</returns>
        [HttpPost("smb")]
        public async Task<IActionResult> CreateSmbListenerAsync([FromBody] CreateSmbRequest request)
        {
            var result = await _listenerService.CreateSmbListenerAsync(request);
            return result.IsSuccess ? Created() : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves the listener with the specified identifier.
        /// </summary>
        /// <param name="ID">The listener identifier.</param>
        /// <returns>An IActionResult returning 200 (OK) with the listener when found; otherwise 404 (Not Found) with an error.</returns>
        [HttpGet("http/{ID}")]
        public async Task<IActionResult> GetListenerByIDAsync([FromRoute] int ID)
        {
            var result = await _listenerService.GetListener(ID);

            return result.IsSuccess ? Ok(result.Response) : NotFound(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^// 
        /// <summary>
        /// Updates the configuration of an existing HTTP listener identified by the specified ID.
        /// </summary>
        /// <param name="ID">Identifier of the HTTP listener to update.</param>
        /// <param name="request">Request object containing the updated listener configuration.</param>
        /// <returns>An IActionResult that returns 200 OK when the update succeeds or 400 Bad Request with an error when it
        /// fails.</returns>
        [HttpPut("http/{ID}")]
        public async Task<IActionResult> UpdateHttpListenerAsync([FromRoute] int ID, [FromBody] UpdateHttpListenerRequest request)
        {
            var result = await _listenerService.UpdateHttpListenerAsync(ID, request.config);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Deletes the listener with the specified identifier and returns an HTTP response indicating success or
        /// failure.
        /// </summary>
        /// <param name="ID">The identifier of the listener to delete.</param>
        /// <returns>An IActionResult that is 200 OK if the deletion succeeds, or 400 Bad Request with an error message if it
        /// fails.</returns>
        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteListenerAsync([FromRoute] int ID) 
        {
            var result = await _listenerService.DeleteListenerAsync(ID);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //