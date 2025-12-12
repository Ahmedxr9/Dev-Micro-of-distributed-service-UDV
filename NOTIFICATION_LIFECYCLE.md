# 🔄 Notification Lifecycle - Complete Flow

## Overview
This document describes the complete lifecycle of a notification from the moment a client sends a request until the notification is fully processed and stored.

---

## 📊 Lifecycle Diagram

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ POST /api/notifications/send
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 1: GatewayService (Port 5000)                    │
│  ┌──────────────────────────────────────────────────┐  │
│  │ NotificationsController.SendNotification()       │  │
│  │                                                   │  │
│  │ 1.1 Validate Request (FluentValidation)          │  │
│  │ 1.2 Create Notification Entity                  │  │
│  │     - Generate GUID                              │  │
│  │     - Status: "Pending"                          │  │
│  │     - Retries: 0                                 │  │
│  │ 1.3 Save to PostgreSQL                           │  │
│  │ 1.4 Create NotificationMessage                   │  │
│  │ 1.5 Publish to RabbitMQ Queue                    │  │
│  │ 1.6 Return Response (notificationId)             │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
       │
       │ Message Published
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 2: RabbitMQ Message Broker                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Queue: email.queue / sms.queue / push.queue     │  │
│  │                                                   │  │
│  │ Message Content:                                  │  │
│  │ {                                                 │  │
│  │   "notificationId": "guid",                     │  │
│  │   "channel": "email",                             │  │
│  │   "recipient": "user@example.com",               │  │
│  │   "message": "Hello!",                           │  │
│  │   "metadata": {...},                             │  │
│  │   "retryCount": 0                                 │  │
│  │ }                                                 │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
       │
       │ Message Consumed
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 3: Channel Service (Email/SMS/Push)               │
│  ┌──────────────────────────────────────────────────┐  │
│  │ EmailWorker / SMSWorker / PushWorker             │  │
│  │                                                   │  │
│  │ 3.1 RabbitMQConsumer.StartConsumingAsync()       │  │
│  │ 3.2 Receive Message from Queue                   │  │
│  │ 3.3 Call ChannelService.ProcessNotificationAsync()│ │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
       │
       │ Process with Retry Policy
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 4: Processing with Retry Logic                   │
│  ┌──────────────────────────────────────────────────┐  │
│  │ EmailService / SMSService / PushService          │  │
│  │                                                   │  │
│  │ Retry Policy:                                     │  │
│  │ - Max Retries: 3                                  │  │
│  │ - Backoff: Exponential (2^retryAttempt seconds)  │  │
│  │                                                   │  │
│  │ For Each Attempt:                                 │  │
│  │ 4.1 Update Status: "Processing"                  │  │
│  │ 4.2 Create Attempt Record                        │  │
│  │ 4.3 Call Provider (SendEmail/SendSMS/SendPush)  │  │
│  │ 4.4 If Success:                                  │  │
│  │     - Update Status: "Sent"                      │  │
│  │     - Update Attempt: "Sent"                     │  │
│  │ 4.5 If Failure:                                   │  │
│  │     - Increment Retries                          │  │
│  │     - Update Attempt: "Failed" + Error           │  │
│  │     - Wait (exponential backoff)                 │  │
│  │     - Retry (up to 3 times)                      │  │
│  │                                                   │  │
│  │ If All Retries Fail:                             │  │
│  │ - Update Status: "Failed"                        │  │
│  │ - Log Error                                      │  │
│  │ - Send to Dead Letter Queue (if configured)     │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
       │
       │ Status Updated
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 5: Database Updates (PostgreSQL)                  │
│  ┌──────────────────────────────────────────────────┐  │
│  │ notifications table:                             │  │
│  │ - status: "Pending" → "Processing" → "Sent"      │  │
│  │ - retries: 0 → 1 → 2 → 3 (if failures)          │  │
│  │ - updatedAt: Timestamp updated                   │  │
│  │                                                   │  │
│  │ notification_attempts table:                     │  │
│  │ - One record per attempt                         │  │
│  │ - Fields: status, errorMessage, retryNumber      │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
       │
       │ Client Queries Status
       ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 6: Status Retrieval                               │
│  ┌──────────────────────────────────────────────────┐  │
│  │ GET /api/notifications/{id}                      │  │
│  │                                                   │  │
│  │ Returns:                                          │  │
│  │ - Current status                                 │  │
│  │ - All attempts                                   │  │
│  │ - Error messages (if any)                        │  │
│  │ - Timestamps                                     │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 🔍 Detailed Step-by-Step Lifecycle

### **Phase 1: Request Reception & Validation** ⏱️ ~50-100ms

#### Step 1.1: Client Sends Request
```http
POST http://localhost:5000/api/notifications/send
Content-Type: application/json

{
  "channel": "email",
  "recipient": "test@example.com",
  "message": "Hello! This is a test notification.",
  "metadata": {
    "subject": "Test Email"
  }
}
```

#### Step 1.2: GatewayService Receives Request
- **Location:** `NotificationsController.SendNotification()`
- **Action:** Request enters the API endpoint

#### Step 1.3: Request Validation
- **Component:** `NotificationRequestValidator`
- **Validations:**
  - ✅ Channel must be: "email", "sms", or "push"
  - ✅ Recipient is required (max 500 chars)
  - ✅ Message is required (max 5000 chars)
- **If Invalid:** Return 400 Bad Request with validation errors
- **If Valid:** Continue to next step

---

### **Phase 2: Database Storage** ⏱️ ~20-50ms

#### Step 2.1: Create Notification Entity
```csharp
var notification = new Notification
{
    Id = Guid.NewGuid(),                    // Generate unique ID
    Channel = "email",                      // From request
    Recipient = "test@example.com",         // From request
    Message = "Hello! This is a test...",  // From request
    Status = "Pending",                     // Initial status
    Retries = 0,                            // No retries yet
    Metadata = "{...}",                     // Serialized metadata
    CreatedAt = DateTime.UtcNow             // Timestamp
};
```

#### Step 2.2: Save to PostgreSQL
- **Component:** `NotificationRepository.CreateAsync()`
- **Action:** Insert into `notifications` table
- **Result:** Notification record created with status "Pending"

**Database State:**
```sql
INSERT INTO notifications (id, channel, recipient, message, status, retries, created_at)
VALUES ('guid', 'email', 'test@example.com', 'Hello!', 'Pending', 0, NOW());
```

---

### **Phase 3: Message Publishing** ⏱️ ~10-30ms

#### Step 3.1: Determine Target Queue
```csharp
var targetQueue = channel switch
{
    "email" => QueueNames.Email,    // "email.queue"
    "sms" => QueueNames.Sms,        // "sms.queue"
    "push" => QueueNames.Push       // "push.queue"
};
```

#### Step 3.2: Create NotificationMessage
```csharp
var message = new NotificationMessage
{
    NotificationId = notification.Id,
    Channel = "email",
    Recipient = "test@example.com",
    Message = "Hello!",
    Metadata = {...},
    RetryCount = 0,
    CreatedAt = notification.CreatedAt
};
```

#### Step 3.3: Publish to RabbitMQ
- **Component:** `RabbitMQProducer.PublishAsync()`
- **Action:** Serialize message to JSON and publish to queue
- **Queue:** `email.queue` (or `sms.queue` / `push.queue`)
- **Exchange:** Default (direct)
- **Routing Key:** Queue name

**RabbitMQ Message:**
```json
{
  "notificationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": "email",
  "recipient": "test@example.com",
  "message": "Hello! This is a test notification.",
  "metadata": {
    "subject": "Test Email"
  },
  "retryCount": 0,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### Step 3.4: Return Response to Client
```json
{
  "notificationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**⏱️ Total Time So Far: ~80-180ms** (Client receives response immediately)

---

### **Phase 4: Message Consumption** ⏱️ ~100-500ms

#### Step 4.1: Channel Service Worker Starts
- **Component:** `EmailWorker` / `SMSWorker` / `PushWorker`
- **Action:** Background service continuously listens to queue
- **Status:** Service is always running, waiting for messages

#### Step 4.2: RabbitMQ Consumer Receives Message
- **Component:** `RabbitMQConsumer.StartConsumingAsync()`
- **Action:** Message is dequeued from RabbitMQ
- **Acknowledgment:** Message is ACK'd after successful processing

#### Step 4.3: Invoke Channel Service
- **Component:** `EmailService.ProcessNotificationAsync()`
- **Action:** Process the notification message

---

### **Phase 5: Processing with Retry Logic** ⏱️ ~1-5 seconds

#### Step 5.1: Retry Policy Setup
```csharp
Retry Policy:
- Max Retries: 3
- Backoff Strategy: Exponential
  - Attempt 1: Wait 2 seconds
  - Attempt 2: Wait 4 seconds
  - Attempt 3: Wait 8 seconds
```

#### Step 5.2: First Attempt

**5.2.1: Update Status to "Processing"**
```sql
UPDATE notifications 
SET status = 'Processing', updated_at = NOW()
WHERE id = 'guid';
```

**5.2.2: Create Attempt Record**
```sql
INSERT INTO notification_attempts 
(id, notification_id, attempted_at, status, retry_number)
VALUES ('new-guid', 'notification-guid', NOW(), 'Processing', 1);
```

**5.2.3: Call Provider**
- **Email:** `MockEmailProvider.SendEmailAsync()`
- **SMS:** `MockSMSProvider.SendSMSAsync()`
- **Push:** `MockPushProvider.SendPushAsync()`

**5.2.4: Success Path**
```sql
-- Update notification status
UPDATE notifications 
SET status = 'Sent', updated_at = NOW()
WHERE id = 'guid';

-- Update attempt status
UPDATE notification_attempts 
SET status = 'Sent', attempted_at = NOW()
WHERE id = 'attempt-guid';
```

**5.2.5: Failure Path**
```sql
-- Increment retries
UPDATE notifications 
SET retries = retries + 1, updated_at = NOW()
WHERE id = 'guid';

-- Update attempt with error
UPDATE notification_attempts 
SET status = 'Failed', error_message = 'Error details'
WHERE id = 'attempt-guid';
```

#### Step 5.3: Retry Attempts (if needed)

**Attempt 2 (after 2 seconds):**
- Same process as Attempt 1
- Retry number: 2
- Wait time: 4 seconds before next retry

**Attempt 3 (after 4 seconds):**
- Same process as Attempt 1
- Retry number: 3
- Wait time: 8 seconds before next retry

#### Step 5.4: Final Failure (if all retries fail)
```sql
-- Update to final status
UPDATE notifications 
SET status = 'Failed', 
    errors = 'All retry attempts failed',
    updated_at = NOW()
WHERE id = 'guid';

-- Final attempt record
INSERT INTO notification_attempts 
(id, notification_id, attempted_at, status, error_message, retry_number)
VALUES ('final-guid', 'notification-guid', NOW(), 'Failed', 'Error', 3);
```

**Message sent to Dead Letter Queue (if configured)**

---

### **Phase 6: Status Tracking** ⏱️ Real-time

#### Step 6.1: Client Queries Status
```http
GET http://localhost:5000/api/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

#### Step 6.2: GatewayService Retrieves Status
- **Component:** `NotificationsController.GetNotificationStatus()`
- **Action:** Query database with notification ID

#### Step 6.3: Return Status Response
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": "email",
  "recipient": "test@example.com",
  "message": "Hello! This is a test notification.",
  "status": "Sent",
  "retries": 0,
  "errors": null,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:01Z",
  "attempts": [
    {
      "attemptedAt": "2024-01-15T10:30:01Z",
      "status": "Sent",
      "errorMessage": null,
      "retryNumber": 1
    }
  ]
}
```

---

## 📊 Status Transitions

```
┌──────────┐
│  Pending │  ← Initial status when created
└────┬─────┘
     │
     │ Message consumed from queue
     ▼
┌──────────────┐
│  Processing  │  ← Status during each attempt
└──────┬───────┘
       │
       ├─────────────────┐
       │                 │
       ▼                 ▼
┌──────────┐      ┌──────────┐
│   Sent   │      │  Failed  │
└──────────┘      └──────────┘
  (Success)        (All retries failed)
```

---

## ⏱️ Timeline Example

### Successful Notification:
```
T+0ms    : Client sends POST request
T+50ms   : Request validated
T+80ms   : Notification saved to DB (status: "Pending")
T+100ms  : Message published to RabbitMQ
T+120ms  : Response returned to client ✅

T+200ms  : EmailWorker consumes message
T+250ms  : Status updated to "Processing"
T+300ms  : Email provider called
T+500ms  : Email sent successfully
T+550ms  : Status updated to "Sent" ✅

Total: ~550ms from request to completion
```

### Failed Notification (with retries):
```
T+0ms    : Client sends POST request
T+120ms  : Response returned to client ✅

T+200ms  : EmailWorker consumes message
T+250ms  : Status: "Processing" (Attempt 1)
T+300ms  : Email provider fails
T+350ms  : Status: "Failed", Retries: 1
T+2350ms : Wait 2 seconds (exponential backoff)
T+2400ms : Status: "Processing" (Attempt 2)
T+2450ms : Email provider fails again
T+2500ms : Status: "Failed", Retries: 2
T+6500ms : Wait 4 seconds
T+6600ms : Status: "Processing" (Attempt 3)
T+6650ms : Email provider fails again
T+6700ms : Status: "Failed", Retries: 3
T+6700ms : Final status: "Failed" ❌

Total: ~6.7 seconds (all retries exhausted)
```

---

## 🗄️ Database State Evolution

### Initial State (After Step 2):
```sql
notifications:
  id: 'guid'
  status: 'Pending'
  retries: 0
  created_at: '2024-01-15 10:30:00'
  updated_at: NULL

notification_attempts: (empty)
```

### During Processing (Step 5.2):
```sql
notifications:
  id: 'guid'
  status: 'Processing'
  retries: 0
  updated_at: '2024-01-15 10:30:00.5'

notification_attempts:
  id: 'attempt-1'
  notification_id: 'guid'
  status: 'Processing'
  retry_number: 1
  attempted_at: '2024-01-15 10:30:00.5'
```

### After Success (Step 5.2.4):
```sql
notifications:
  id: 'guid'
  status: 'Sent'
  retries: 0
  updated_at: '2024-01-15 10:30:01'

notification_attempts:
  id: 'attempt-1'
  notification_id: 'guid'
  status: 'Sent'
  retry_number: 1
  attempted_at: '2024-01-15 10:30:01'
```

### After Failure (Step 5.4):
```sql
notifications:
  id: 'guid'
  status: 'Failed'
  retries: 3
  errors: 'All retry attempts failed'
  updated_at: '2024-01-15 10:30:06'

notification_attempts:
  id: 'attempt-1', status: 'Failed', retry_number: 1
  id: 'attempt-2', status: 'Failed', retry_number: 2
  id: 'attempt-3', status: 'Failed', retry_number: 3
```

---

## 🔄 Retry Logic Details

### Retry Policy Configuration:
```csharp
Max Retries: 3
Backoff: Exponential (2^retryAttempt seconds)
  - Retry 1: Wait 2 seconds (2^1)
  - Retry 2: Wait 4 seconds (2^2)
  - Retry 3: Wait 8 seconds (2^3)
```

### Retry Flow:
```
Attempt 1 → Fail → Wait 2s → Attempt 2 → Fail → Wait 4s → Attempt 3 → Fail → Final Failure
```

### Retry Conditions:
- ✅ All exceptions trigger retry
- ✅ Maximum 3 retries
- ✅ Exponential backoff between retries
- ✅ Each attempt logged in database

---

## 📝 Key Components Involved

1. **GatewayService**
   - `NotificationsController` - API endpoints
   - `NotificationRequestValidator` - Validation
   - `RabbitMQProducer` - Message publishing

2. **RabbitMQ**
   - Message broker
   - Queue routing
   - Dead letter exchange (for failed messages)

3. **Channel Services** (Email/SMS/Push)
   - `*Worker` - Background service
   - `RabbitMQConsumer` - Message consumption
   - `*Service` - Business logic
   - `*Provider` - External provider interface

4. **PostgreSQL**
   - `notifications` table - Main records
   - `notification_attempts` table - Attempt history

5. **Monitoring**
   - Serilog - Logging
   - Prometheus - Metrics
   - Elasticsearch - Log storage

---

## 🎯 Key Points

1. **Asynchronous Processing**: Client receives response immediately (~120ms), processing happens in background
2. **Idempotency**: Each notification has unique ID, can be queried anytime
3. **Retry Logic**: Automatic retry with exponential backoff
4. **Status Tracking**: Complete history of all attempts
5. **Error Handling**: Comprehensive error logging and tracking
6. **Scalability**: Message queue allows horizontal scaling

---

## 🔍 Monitoring & Observability

### Logs (Serilog → Elasticsearch):
- Request received
- Validation results
- Database operations
- Message publishing
- Processing attempts
- Success/failure events

### Metrics (Prometheus):
- `*_notifications_processed_total` - Success counter
- `*_notifications_failed_total` - Failure counter
- `*_notification_processing_duration_seconds` - Processing time

### Health Checks:
- `/health` - Overall health
- `/health/ready` - Readiness (checks dependencies)
- `/health/live` - Liveness probe

---

**Last Updated:** 2024-12-12

