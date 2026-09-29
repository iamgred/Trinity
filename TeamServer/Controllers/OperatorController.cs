//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.Models;

namespace TeamServer.Controllers
{
    [Route(Routes.Operators)]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        private readonly OperatorService _operatorService;
        /// <summary>
        /// Initializes a new instance of the OperatorController class with the specified OperatorService.
        /// </summary>
        /// <param name="operatorService"></param>
        public OperatorController(OperatorService operatorService)
        {
            _operatorService = operatorService;
        }
        /// <summary>
        /// Gets a list of all operators.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetOperatorsAsync()
        {
            var result = await _operatorService.GetOperatorsAsync();
            return Ok(result);
        }
        /// <summary>
        /// Gets an operator by its ID.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpGet("{operatorID}")]
        public async Task<IActionResult> GetOperatorByIdAsync(int operatorID)
        {
            var result = await _operatorService.GetOperatorByIdAsync(operatorID);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Creates a new operator.
        /// </summary>
        /// <param name="newOperator"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOperatorAsync([FromBody] Operator newOperator)
        {
            var result = await _operatorService.CreateOperatorAsync(newOperator);
            return CreatedAtAction(nameof(GetOperatorByIdAsync), new { operatorID = result }, newOperator);
        }
        /// <summary>
        /// Updates an existing operator by its ID.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="updatedOperator"></param>
        /// <returns></returns>
        [HttpPut("{operatorID}")]
        public async Task<IActionResult> UpdateOperatorAsync(int operatorID, [FromBody] Operator updatedOperator)
        {
            if (operatorID != updatedOperator.ID)
            {
                return BadRequest("Operator ID mismatch.");
            }
            updatedOperator.ID = operatorID; // Ensure the ID is set correctly
            var result = await _operatorService.UpdateOperatorAsync(updatedOperator);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Deletes an operator by its ID.
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        [HttpDelete("{operatorID}")]
        public async Task<IActionResult> DeleteOperatorAsync(int operatorID)
        {
            var result = await _operatorService.DeleteOperatorAsync(operatorID);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //