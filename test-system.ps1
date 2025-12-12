# Test Script for Notification System
# This script tests the Gateway Service API

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Notification System Test Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# Test 1: Health Check
Write-Host "Test 1: Checking service health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -ErrorAction Stop
    Write-Host "OK Service is healthy: $health" -ForegroundColor Green
} catch {
    Write-Host "X Service is not responding. Is it running?" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 2: Send Email Notification
Write-Host "Test 2: Sending email notification..." -ForegroundColor Yellow
try {
    $emailBody = @{
        channel = "email"
        recipient = "test@example.com"
        message = "Hello! This is a test email notification from the automated test script."
        metadata = @{
            subject = "Test Email"
            priority = "high"
        }
    } | ConvertTo-Json -Depth 3

    $emailResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/send" `
        -Method Post `
        -ContentType "application/json" `
        -Body $emailBody `
        -ErrorAction Stop

    Write-Host "OK Email notification sent successfully!" -ForegroundColor Green
    Write-Host "  Notification ID: $($emailResponse.notificationId)" -ForegroundColor Cyan
    Write-Host "  Status: $($emailResponse.status)" -ForegroundColor Cyan
    Write-Host "  Created: $($emailResponse.createdAt)" -ForegroundColor Cyan
    
    $emailNotificationId = $emailResponse.notificationId
} catch {
    Write-Host "X Failed to send email notification" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    $emailNotificationId = $null
}
Write-Host ""

# Test 3: Send SMS Notification
Write-Host "Test 3: Sending SMS notification..." -ForegroundColor Yellow
try {
    $smsBody = @{
        channel = "sms"
        recipient = "+1234567890"
        message = "Hello! This is a test SMS notification."
    } | ConvertTo-Json

    $smsResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/send" `
        -Method Post `
        -ContentType "application/json" `
        -Body $smsBody `
        -ErrorAction Stop

    Write-Host "OK SMS notification sent successfully!" -ForegroundColor Green
    Write-Host "  Notification ID: $($smsResponse.notificationId)" -ForegroundColor Cyan
    Write-Host "  Status: $($smsResponse.status)" -ForegroundColor Cyan
    
    $smsNotificationId = $smsResponse.notificationId
} catch {
    Write-Host "X Failed to send SMS notification" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    $smsNotificationId = $null
}
Write-Host ""

# Test 4: Send Push Notification
Write-Host "Test 4: Sending push notification..." -ForegroundColor Yellow
try {
    $pushBody = @{
        channel = "push"
        recipient = "device-token-test-12345"
        message = "Hello! This is a test push notification."
        metadata = @{
            title = "Test Notification"
            badge = 1
        }
    } | ConvertTo-Json -Depth 3

    $pushResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/send" `
        -Method Post `
        -ContentType "application/json" `
        -Body $pushBody `
        -ErrorAction Stop

    Write-Host "OK Push notification sent successfully!" -ForegroundColor Green
    Write-Host "  Notification ID: $($pushResponse.notificationId)" -ForegroundColor Cyan
    Write-Host "  Status: $($pushResponse.status)" -ForegroundColor Cyan
    
    $pushNotificationId = $pushResponse.notificationId
} catch {
    Write-Host "X Failed to send push notification" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    $pushNotificationId = $null
}
Write-Host ""

# Test 5: Check Notification Status
if ($emailNotificationId) {
    Write-Host "Test 5: Checking email notification status..." -ForegroundColor Yellow
    try {
        Start-Sleep -Seconds 2  # Wait a bit for processing
        $status = Invoke-RestMethod -Uri "$baseUrl/api/notifications/$emailNotificationId" -ErrorAction Stop
        
        Write-Host "OK Status retrieved successfully!" -ForegroundColor Green
        Write-Host "  ID: $($status.id)" -ForegroundColor Cyan
        Write-Host "  Channel: $($status.channel)" -ForegroundColor Cyan
        Write-Host "  Recipient: $($status.recipient)" -ForegroundColor Cyan
        Write-Host "  Status: $($status.status)" -ForegroundColor $(if($status.status -eq "Sent"){"Green"}else{"Yellow"})
        Write-Host "  Retries: $($status.retries)" -ForegroundColor Cyan
        
        if ($status.attempts -and $status.attempts.Count -gt 0) {
            Write-Host "  Attempts:" -ForegroundColor Cyan
            foreach ($attempt in $status.attempts) {
                Write-Host "    - Attempt #$($attempt.retryNumber): $($attempt.status) at $($attempt.attemptedAt)" -ForegroundColor Gray
            }
        }
    } catch {
        Write-Host "X Failed to retrieve status" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 6: Invalid Request (Error Handling)
Write-Host "Test 6: Testing error handling with invalid channel..." -ForegroundColor Yellow
try {
    $invalidBody = @{
        channel = "invalid_channel"
        recipient = "test@example.com"
        message = "This should fail"
    } | ConvertTo-Json

    $invalidResponse = Invoke-RestMethod -Uri "$baseUrl/api/notifications/send" `
        -Method Post `
        -ContentType "application/json" `
        -Body $invalidBody `
        -ErrorAction Stop
    
    Write-Host "X Expected error but got success" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "OK Error handling works correctly (400 Bad Request)" -ForegroundColor Green
    } else {
        Write-Host "X Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Service URL: $baseUrl" -ForegroundColor White
Write-Host "Swagger UI: $baseUrl/swagger" -ForegroundColor White
Write-Host "Health Check: $baseUrl/health" -ForegroundColor White
Write-Host "Metrics: $baseUrl/metrics" -ForegroundColor White
Write-Host ""
Write-Host "All tests completed! Check the results above." -ForegroundColor Green
Write-Host ""
