using CryptoExchangeTask.Application.Abstractions;
using CryptoExchangeTask.Application.DTOs;
using CryptoExchangeTask.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace CryptoExchangeTask.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionPlanController : ControllerBase
{
    private readonly ILogger<ExecutionPlanController> _logger;

    private readonly IExecutionPlanService _executionPlanService;

    public ExecutionPlanController(ILogger<ExecutionPlanController> logger, IExecutionPlanService executionPlanService)
    {
        _logger = logger;
        _executionPlanService = executionPlanService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExecutionPlanDto), StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Get(
        [FromQuery, Required, Description("Specifies the type of the order")] OrderType orderType,
        [FromQuery, Required, Range(0, double.MaxValue), Description("Specifies the amount of the order in decimal format")] decimal orderAmount)
    {
        try
        {
            var request = new GetExecutionPlanRequest
            {
                OrderType = orderType,
                OrderAmount = orderAmount
            };

            var executionPlan = _executionPlanService.GetExecutionPlan(request);

            return Ok(executionPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the order.");
            return BadRequest(new ProblemDetails
            {
                Title = "An error occurred while processing the order.",
                Detail = ex.Message,
            });
        }
    }
}
