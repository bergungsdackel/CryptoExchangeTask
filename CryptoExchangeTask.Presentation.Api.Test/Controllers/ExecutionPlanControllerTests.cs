using CryptoExchangeTask.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace CryptoExchangeTask.Presentation.Api.Test.Controllers
{
    public class ExecutionPlanControllerTests : WebApplicationFactory<Program> 
    {
        private const string ExecutionPlanApiUrl = "/api/executionplan";

        private HttpClient _apiClient;

        [SetUp]
        public void Setup()
        {
            _apiClient = CreateClient();
        }

        [Test]
        public async Task It_should_return_execution_plan_when_request_is_valid()
        {
            // Arrange
            var orderType = "buy";
            var orderAmount = "1.5";
            var url = $"{ExecutionPlanApiUrl}?orderType={orderType}&orderAmount={orderAmount}";

            // Act
            var response = await _apiClient.GetAsync(url);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var executionPlan = await response.Content.ReadFromJsonAsync<ExecutionPlanDto>();
            executionPlan.ShouldNotBeNull();
            executionPlan.Orders.ShouldNotBeEmpty();
            executionPlan.TotalCost.ShouldBeGreaterThan(0);
        }

        [Test]
        public async Task It_should_return_bad_request_when_order_type_is_invalid()
        {
            // Arrange
            var orderType = "invalid";
            var orderAmount = "1.5";
            var url = $"{ExecutionPlanApiUrl}?orderType={orderType}&orderAmount={orderAmount}";

            // Act
            var response = await _apiClient.GetAsync(url);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problemDetails.ShouldNotBeNull();
        }   

        [TestCase(-1.5555)]
        [TestCase(-1337)]
        [TestCase(0)]
        public async Task It_should_return_bad_request_when_order_amount_is_invalid(decimal orderAmount)
        {
            // Arrange
            var orderType = "buy";
            var url = $"{ExecutionPlanApiUrl}?orderType={orderType}&orderAmount={orderAmount}";

            // Act
            var response = await _apiClient.GetAsync(url);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problemDetails.ShouldNotBeNull();
        }
    }
}