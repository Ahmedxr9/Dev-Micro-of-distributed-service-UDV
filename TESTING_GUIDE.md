# 🧪 Testing Guide - Notification System

## 📋 What This Project Does

This is a **Distributed Microservice-Based Notification System** that allows you to send notifications through multiple channels:

### **Core Functionality:**
1. **Gateway Service** - Receives notification requests via REST API
2. **Message Queue** - Uses RabbitMQ to route notifications to appropriate services
3. **Channel Services** - Process notifications:
   - **Email Service** - Sends email notifications
   - **SMS Service** - Sends SMS notifications  
   - **Push Service** - Sends push notifications
4. **Database** - Stores notification metadata and status (PostgreSQL)
5. **Monitoring** - Prometheus metrics and health checks

### **How It Works:**
```
Client → Gateway API → RabbitMQ → Channel Service → Provider (Mock)
                ↓
         PostgreSQL (stores status)
```

---

## 🚀 Quick Start Testing

### **Prerequisites:**
- Gateway Service running (port 5000)
- Infrastructure services running (PostgreSQL, RabbitMQ, Elasticsearch)
- Channel services running (Email, SMS, Push) - Optional for full testing

---

## 🧪 Method 1: Using Swagger UI (Easiest)

### Step 1: Open Swagger UI
1. Open your browser and go to: **http://localhost:5000/swagger**
2. You'll see the API documentation with all available endpoints

### Step 2: Test Send Notification
1. Find the `POST /api/notifications/send` endpoint
2. Click **"Try it out"**
3. Enter this test request:
```json
{
  "channel": "email",
  "recipient": "test@example.com",
  "message": "Hello! This is a test notification from the system.",
  "metadata": {
    "subject": "Test Notification",
    "priority": "high"
  }
}
```
4. Click **"Execute"**
5. Copy the `notificationId` from the response

### Step 3: Check Notification Status
1. Find the `GET /api/notifications/{id}` endpoint
2. Click **"Try it out"**
3. Paste the `notificationId` from Step 2
4. Click **"Execute"**
5. View the notification status and details

---

## 🧪 Method 2: Using PowerShell (Windows)

### Test 1: Send Email Notification
```powershell
$body = @{
    channel = "email"
    recipient = "user@example.com"
    message = "Hello! This is a test email notification."
    metadata = @{
        subject = "Test Email"
        priority = "normal"
    }
} | ConvertTo-Json -Depth 3

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/send" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

Write-Host "Notification ID: $($response.notificationId)" -ForegroundColor Green
Write-Host "Status: $($response.status)" -ForegroundColor Cyan
```

### Test 2: Send SMS Notification
```powershell
$body = @{
    channel = "sms"
    recipient = "+1234567890"
    message = "Hello! This is a test SMS notification."
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/send" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

$notificationId = $response.notificationId
Write-Host "SMS Notification ID: $notificationId" -ForegroundColor Green
```

### Test 3: Send Push Notification
```powershell
$body = @{
    channel = "push"
    recipient = "device-token-12345"
    message = "Hello! This is a test push notification."
    metadata = @{
        title = "New Notification"
        badge = 1
    }
} | ConvertTo-Json -Depth 3

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/send" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

$notificationId = $response.notificationId
Write-Host "Push Notification ID: $notificationId" -ForegroundColor Green
```

### Test 4: Check Notification Status
```powershell
# Replace with actual notification ID from previous tests
$notificationId = "your-notification-id-here"

$status = Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/$notificationId"

Write-Host "`n=== Notification Status ===" -ForegroundColor Yellow
Write-Host "ID: $($status.id)"
Write-Host "Channel: $($status.channel)"
Write-Host "Recipient: $($status.recipient)"
Write-Host "Status: $($status.status)" -ForegroundColor $(if($status.status -eq "Sent"){"Green"}else{"Yellow"})
Write-Host "Retries: $($status.retries)"
Write-Host "Created: $($status.createdAt)"
if ($status.attempts) {
    Write-Host "`nAttempts:" -ForegroundColor Cyan
    $status.attempts | ForEach-Object {
        Write-Host "  - Attempt #$($_.retryNumber): $($_.status) at $($_.attemptedAt)"
    }
}
```

### Test 5: Health Check
```powershell
$health = Invoke-RestMethod -Uri "http://localhost:5000/health"
Write-Host "Service Health: $health" -ForegroundColor Green
```

---

## 🧪 Method 3: Using cURL (Cross-platform)

### Send Email Notification
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "email",
    "recipient": "test@example.com",
    "message": "Hello! This is a test email notification.",
    "metadata": {
      "subject": "Test Email",
      "priority": "high"
    }
  }'
```

### Send SMS Notification
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "sms",
    "recipient": "+1234567890",
    "message": "Hello! This is a test SMS notification."
  }'
```

### Send Push Notification
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "push",
    "recipient": "device-token-123",
    "message": "Hello! This is a test push notification."
  }'
```

### Check Notification Status
```bash
# Replace {notification-id} with actual ID from send response
curl http://localhost:5000/api/notifications/{notification-id}
```

---

## 🧪 Method 4: Automated Test Script

Run the provided PowerShell test script:
```powershell
.\test-system.ps1
```

---

## ✅ What to Verify

### 1. **Gateway Service is Running**
- Health check: `http://localhost:5000/health` returns "Healthy"
- Swagger UI: `http://localhost:5000/swagger` is accessible

### 2. **Send Notification Works**
- POST request returns 200 OK
- Response contains `notificationId`, `status: "Pending"`, and `createdAt`

### 3. **Notification is Stored**
- GET request with notification ID returns notification details
- Status shows in database

### 4. **Message is Queued**
- Check RabbitMQ Management UI: `http://localhost:15672` (guest/guest)
- Verify message appears in appropriate queue (email.queue, sms.queue, or push.queue)

### 5. **Channel Service Processes** (if running)
- Notification status changes from "Pending" to "Sent" or "Failed"
- Check service logs for processing confirmation

---

## 🔍 Monitoring & Debugging

### Check Service Health
- Gateway: `http://localhost:5000/health`
- Email Service: `http://localhost:5001/health` (if running)
- SMS Service: `http://localhost:5002/health` (if running)
- Push Service: `http://localhost:5003/health` (if running)

### View Metrics
- Prometheus: `http://localhost:9090`
- Service Metrics: `http://localhost:5000/metrics`

### Check RabbitMQ Queues
- Management UI: `http://localhost:15672`
- Login: guest / guest
- View queues: email.queue, sms.queue, push.queue

### View Logs
- Elasticsearch: `http://localhost:9200`
- Kibana: `http://localhost:5601`

---

## 📊 Expected Behavior

### Successful Flow:
1. **Send Request** → Returns notification ID with status "Pending"
2. **Message Queued** → Appears in RabbitMQ queue
3. **Service Processes** → Channel service picks up message
4. **Status Updates** → Changes to "Sent" or "Failed"
5. **Retry Logic** → If failed, retries up to 3 times with exponential backoff

### Response Times:
- API Response: < 100ms (immediate)
- Message Processing: 1-5 seconds (depends on channel service)
- Status Update: Available immediately after processing

---

## 🐛 Troubleshooting

### Issue: "Failed to connect to database"
**Solution:** Ensure PostgreSQL is running:
```powershell
cd infrastructure
docker-compose up -d postgres
```

### Issue: "RabbitMQ connection failed"
**Solution:** Ensure RabbitMQ is running:
```powershell
cd infrastructure
docker-compose up -d rabbitmq
```

### Issue: Notification stays in "Pending" status
**Solution:** 
- Check if channel services are running
- Verify RabbitMQ queues have consumers
- Check service logs for errors

### Issue: Swagger UI not accessible
**Solution:** 
- Ensure service is running in Development environment
- Check if port 5000 is available
- Verify firewall settings

---

## 🎯 Test Scenarios

### Scenario 1: Send Email Notification
```json
POST /api/notifications/send
{
  "channel": "email",
  "recipient": "user@example.com",
  "message": "Welcome to our service!",
  "metadata": {
    "subject": "Welcome Email",
    "template": "welcome"
  }
}
```

### Scenario 2: Send SMS with Retry
```json
POST /api/notifications/send
{
  "channel": "sms",
  "recipient": "+1234567890",
  "message": "Your verification code is 123456"
}
```

### Scenario 3: Send Push Notification
```json
POST /api/notifications/send
{
  "channel": "push",
  "recipient": "device-token-abc123",
  "message": "You have a new message",
  "metadata": {
    "title": "New Message",
    "badge": 1,
    "sound": "default"
  }
}
```

### Scenario 4: Invalid Channel (Error Test)
```json
POST /api/notifications/send
{
  "channel": "invalid",
  "recipient": "test@example.com",
  "message": "This should fail"
}
```
**Expected:** 400 Bad Request with validation error

---

## 📝 Notes

- All providers are **Mock implementations** (for testing/demo purposes)
- Notifications are processed **asynchronously** via message queues
- Status updates happen in **real-time** as services process messages
- Database automatically creates tables on first run
- Retry logic handles temporary failures automatically

---

## 🎉 Success Indicators

✅ **System is working correctly if:**
- Health check returns "Healthy"
- Send notification returns 200 OK with notification ID
- Notification status can be retrieved
- Messages appear in RabbitMQ queues
- Database contains notification records
- Channel services process messages (if running)
- Status updates from "Pending" to "Sent" or "Failed"

---

**Happy Testing! 🚀**
