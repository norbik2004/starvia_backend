using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.IO;
using Web.Helpers;
using Core.DTO.Stripe;
using Core.DTO.Stripe.Request;
using Core.Entities;
using Core.Services;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StripeController(IStripeService stripeService, ILogger<StripeController> logger) : ControllerBase
{

    [Authorize]
    [HttpPost("subscripe-to-postly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        

        var userId = UserHelpers.GetUserIdFromClaims(User);

        logger.LogInformation("Creating Stripe checkout session for user {UserId}", userId);

        var result = await stripeService.CreateCheckoutSessionAsync(userId);

        return Ok(new { sessionId = result.SessionId, url = result.Url });
    }

    [Authorize]
    [HttpPost("create-portal-session")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePortalSession([FromBody] CreatePortalSessionRequest request)
    {
        var userId = UserHelpers.GetUserIdFromClaims(User);

        try
        {
            var result = await stripeService.CreatePortalSessionAsync(userId, request.ReturnUrl);
            return Ok(new { url = result.Url });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Stripe wymaga surowego body do walidacji podpisu — bez [Authorize]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        logger.LogInformation("Received Stripe webhook: {Json}", json);

        try
        {
            await stripeService.HandleWebhookAsync(json, signatureHeader);
            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}