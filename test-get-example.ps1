# Example: Testing GET /api/notifications/{id}
# This script shows how to send a notification and then retrieve its status

$baseUrl = "http://localhost:5000"

Write-Host "=== GET /api/notifications/{id} Example ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Send a notification to get an ID
Write-Host "Step 1: Sending a notification..." -ForegroundColor Yellow

$sendBody = @{
    channel = "email"
    recipient = "test@example.com"
    message = "Hello! This is a test notification to demonstrate GET endpoint."
    metadata = @{
        subject = "Test Notification"
        priority = "normal"
    }
} | ConvertTo-Json -Depth 3

try {
    $sendResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/send" `
        -Method Post `
        -ContentType "application/json" `
        -Body $sendBody
    
    $notificationId = $sendResponse.notificationId
    
    Write-Host "✓ Notification sent successfully!" -ForegroundColor Green
    Write-Host "  Notification ID: $notificationId" -ForegroundColor Cyan
    Write-Host "  Status: $($sendResponse.status)" -ForegroundColor Cyan
    Write-Host ""
    
    # Step 2: Wait a moment for processing
    Write-Host "Waiting 2 seconds for processing..." -ForegroundColor Gray
    Start-Sleep -Seconds 2
    
    # Step 3: Get notification status using the ID
    Write-Host "Step 2: Getting notification status..." -ForegroundColor Yellow
    
    $statusResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/$notificationId"
    
    Write-Host "✓ Status retrieved successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "=== Notification Details ===" -ForegroundColor Cyan
    Write-Host "ID: $($statusResponse.id)" -ForegroundColor White
    Write-Host "Channel: $($statusResponse.channel)" -ForegroundColor White
    Write-Host "Recipient: $($statusResponse.recipient)" -ForegroundColor White
    Write-Host "Message: $($statusResponse.message)" -ForegroundColor White
    Write-Host "Status: $($statusResponse.status)" -ForegroundColor $(if($statusResponse.status -eq "Sent"){"Green"}else{"Yellow"})
    Write-Host "Retries: $($statusResponse.retries)" -ForegroundColor White
    Write-Host "Created: $($statusResponse.createdAt)" -ForegroundColor White
    
    if ($statusResponse.updatedAt) {
        Write-Host "Updated: $($statusResponse.updatedAt)" -ForegroundColor White
    }
    
    if ($statusResponse.attempts -and $statusResponse.attempts.Count -gt 0) {
        Write-Host ""
        Write-Host "=== Processing Attempts ===" -ForegroundColor Cyan
        foreach ($attempt in $statusResponse.attempts) {
            $statusColor = if($attempt.status -eq "Sent"){"Green"}elseif($attempt.status -eq "Failed"){"Red"}else{"Yellow"}
            Write-Host "  Attempt #$($attempt.retryNumber): $($attempt.status) at $($attempt.attemptedAt)" -ForegroundColor $statusColor
            if ($attempt.errorMessage) {
                Write-Host "    Error: $($attempt.errorMessage)" -ForegroundColor Red
            }
        }
    }
    
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "  Notification not found. Make sure the ID is correct." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Example URLs ===" -ForegroundColor Cyan
Write-Host "Full URL: $baseUrl/api/notifications/$notificationId" -ForegroundColor White
Write-Host ""

