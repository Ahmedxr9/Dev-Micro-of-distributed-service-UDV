using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Database;
using Shared.Messaging;
using Shared.Models;
using AutoMapper;
using FluentValidation;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _repository;
    private readonly IMessageProducer _messageProducer;
    private readonly IMapper _mapper;
    private readonly IValidator<NotificationRequestDto> _validator;
    private readonly ILogger<NotificationsController> _logger;
    private static readonly ActivitySource ActivitySource = new("NotificationGateway");

    public NotificationsController(
        INotificationRepository repository,
        IMessageProducer messageProducer,
        IMapper mapper,
        IValidator<NotificationRequestDto> validator,
        ILogger<NotificationsController> logger)
    {
        _repository = repository;
        _messageProducer = messageProducer;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> SendNotification([FromBody] NotificationRequestDto request)
    {
        using var activity = ActivitySource.StartActivity("SendNotification");
        activity?.SetTag("channel", request.Channel);
        activity?.SetTag("recipient", request.Recipient);

        // Validate request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for notification request: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(validationResult.Errors);
        }

        try
        {
            // Create notification record
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Channel = request.Channel.ToLower(),
                Recipient = request.Recipient,
                Message = request.Message,
                Status = "Pending",
                Retries = 0,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow
            };

            notification = await _repository.CreateAsync(notification);
            activity?.SetTag("notificationId", notification.Id.ToString());

            // Determine target queue
            var targetQueue = request.Channel.ToLower() switch
            {
                "email" => QueueNames.Email,
                "sms" => QueueNames.Sms,
                "push" => QueueNames.Push,
                _ => throw new ArgumentException($"Unsupported channel: {request.Channel}")
            };

            // Create message
            var message = new NotificationMessage
            {
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Recipient = notification.Recipient,
                Message = notification.Message,
                Metadata = request.Metadata,
                RetryCount = 0,
                CreatedAt = notification.CreatedAt
            };

            // Publish to message broker
            await _messageProducer.PublishAsync(message, targetQueue);
            
            _logger.LogInformation("Notification {NotificationId} published to queue {Queue}", 
                notification.Id, targetQueue);

            var response = _mapper.Map<NotificationResponseDto>(notification);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationStatusDto>> GetNotificationStatus(Guid id)
    {
        using var activity = ActivitySource.StartActivity("GetNotificationStatus");
        activity?.SetTag("notificationId", id.ToString());

        var notification = await _repository.GetByIdWithAttemptsAsync(id);
        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found", id);
            return NotFound(new { error = "Notification not found" });
        }

        var response = new NotificationStatusDto
        {
            Id = notification.Id,
            Channel = notification.Channel,
            Recipient = notification.Recipient,
            Message = notification.Message,
            Status = notification.Status,
            Retries = notification.Retries,
            Errors = notification.Errors,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt,
            Attempts = notification.Attempts.Select(a => new AttemptDto
            {
                AttemptedAt = a.AttemptedAt,
                Status = a.Status,
                ErrorMessage = a.ErrorMessage,
                RetryNumber = a.RetryNumber
            }).OrderBy(a => a.AttemptedAt).ToList()
        };

        return Ok(response);
    }
}

