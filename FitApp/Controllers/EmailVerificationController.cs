using Microsoft.AspNetCore.Mvc;
using FitApp.Services;
using FitApp.DTOs;

namespace FitApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ILogger<EmailVerificationController> _logger;

        public EmailVerificationController(
            IEmailVerificationService emailVerificationService,
            ILogger<EmailVerificationController> logger)
        {
            _emailVerificationService = emailVerificationService;
            _logger = logger;
        }

        // POST: api/auth/register/start
        [HttpPost("register/start")]
        public async Task<ActionResult<RegisterStartResponseDto>> StartRegistration(RegisterStartDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Validation failed", errors, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                var clientIp = GetClientIpAddress();
                var result = await _emailVerificationService.StartRegistrationAsync(request, clientIp);

                if (result.Message.Contains("already exists"))
                {
                    return Conflict(new { error = result.Message, statusCode = 409, timestamp = DateTime.UtcNow });
                }

                if (result.Message.Contains("Too many"))
                {
                    return StatusCode(429, new { error = result.Message, statusCode = 429, timestamp = DateTime.UtcNow });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartRegistration for email {Email}", request.Email);
                return StatusCode(500, new { error = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/auth/register/verify
        [HttpPost("register/verify")]
        public async Task<ActionResult<RegisterVerifyResponseDto>> VerifyRegistration(RegisterVerifyDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Validation failed", errors, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                var result = await _emailVerificationService.VerifyRegistrationAsync(request);

                if (result.Message.Contains("Invalid") || result.Message.Contains("expired") || result.Message.Contains("attempts"))
                {
                    return BadRequest(new { error = result.Message, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                if (result.Message.Contains("already been used"))
                {
                    return Conflict(new { error = result.Message, statusCode = 409, timestamp = DateTime.UtcNow });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyRegistration for verification ID {VerificationId}", request.VerificationId);
                return StatusCode(500, new { error = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/auth/register/resend
        [HttpPost("register/resend")]
        public async Task<ActionResult<RegisterResendResponseDto>> ResendVerification(RegisterResendDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Validation failed", errors, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                var clientIp = GetClientIpAddress();
                var result = await _emailVerificationService.ResendVerificationAsync(request, clientIp);

                if (result.Message.Contains("Invalid"))
                {
                    return BadRequest(new { error = result.Message, statusCode = 400, timestamp = DateTime.UtcNow });
                }

                if (result.Message.Contains("Too many"))
                {
                    return StatusCode(429, new { error = result.Message, statusCode = 429, timestamp = DateTime.UtcNow });
                }

                if (result.Message.Contains("already been used"))
                {
                    return Conflict(new { error = result.Message, statusCode = 409, timestamp = DateTime.UtcNow });
                }

                if (result.Message.Contains("Please wait"))
                {
                    return StatusCode(429, new { error = result.Message, statusCode = 429, timestamp = DateTime.UtcNow });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResendVerification for verification ID {VerificationId}", request.VerificationId);
                return StatusCode(500, new { error = "Internal server error", statusCode = 500, timestamp = DateTime.UtcNow });
            }
        }

        private string GetClientIpAddress()
        {
            var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            return remoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
