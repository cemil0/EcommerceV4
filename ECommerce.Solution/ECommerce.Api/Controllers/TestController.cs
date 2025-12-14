using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test endpoint - throws exception to test error handling
    /// </summary>
    /// <response code="500">Internal server error with formatted response</response>
    [HttpGet("error")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Test error handling", Description = "Throws an exception to test global exception handling middleware")]
    public IActionResult ThrowError()
    {
        throw new InvalidOperationException("This is a test exception to verify error handling middleware.");
    }

    /// <summary>
    /// Test endpoint - throws not found exception
    /// </summary>
    /// <response code="404">Not found with formatted response</response>
    [HttpGet("not-found")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Test not found error", Description = "Throws a KeyNotFoundException to test 404 handling")]
    public IActionResult ThrowNotFound()
    {
        throw new KeyNotFoundException("Resource not found - testing 404 error handling.");
    }

    /// <summary>
    /// Test endpoint - throws bad request exception
    /// </summary>
    /// <response code="400">Bad request with formatted response</response>
    [HttpGet("bad-request")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Test bad request error", Description = "Throws an ArgumentException to test 400 handling")]
    public IActionResult ThrowBadRequest()
    {
        throw new ArgumentException("Invalid argument - testing 400 error handling.");
    }
}
