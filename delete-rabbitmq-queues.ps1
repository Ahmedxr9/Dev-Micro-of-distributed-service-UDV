# Script to delete RabbitMQ queues that have incorrect configuration
# This fixes the PRECONDITION_FAILED error

Write-Host "Deleting RabbitMQ queues to fix configuration mismatch..." -ForegroundColor Yellow
Write-Host ""

$baseUrl = "http://localhost:15672"
$username = "guest"
$password = "guest"

# Queues to delete
$queues = @(
    "email.queue",
    "sms.queue",
    "push.queue",
    "notifications.queue"
)

$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))

foreach ($queue in $queues) {
    try {
        $headers = @{
            Authorization = "Basic $credentials"
        }
        
        # Delete queue
        $response = Invoke-RestMethod -Uri "$baseUrl/api/queues/%2f/$queue" `
            -Method Delete `
            -Headers $headers `
            -ErrorAction Stop
        
        Write-Host "OK Deleted queue: $queue" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "  Queue $queue does not exist (already deleted or never created)" -ForegroundColor Gray
        } else {
            Write-Host "X Failed to delete queue: $queue" -ForegroundColor Red
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Done! Queues will be recreated with correct configuration on next service start." -ForegroundColor Cyan
Write-Host ""
