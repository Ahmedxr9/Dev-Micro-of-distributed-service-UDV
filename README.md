# Distributed Microservice-Based Notification System

A production-ready, scalable notification platform built with .NET 8, RabbitMQ, PostgreSQL, Docker, Prometheus, and Grafana. This system supports Email, SMS, and Push notifications with an extensible architecture for future channels.

## 🏗️ Architecture Overview

### System Architecture

```
┌─────────────────┐
│   Client Apps   │
└────────┬─────────┘
         │
         │ HTTP/REST
         ▼
┌─────────────────────────────────────┐
│      Gateway Service                │
│  - Request Validation               │
│  - Metadata Storage                 │
│  - Message Publishing               │
│  - Health Checks                    │
└────────┬────────────────────────────┘
         │
         │ RabbitMQ
         ▼
┌─────────────────────────────────────┐
│         RabbitMQ Broker             │
│  - notifications.queue              │
│  - email.queue                      │
│  - sms.queue                        │
│  - push.queue                       │
│  - Dead Letter Exchange             │
└────┬────┬────┬──────────────────────┘
     │    │    │
     │    │    │
     ▼    ▼    ▼
┌──────┐ ┌──────┐ ┌──────┐
│Email │ │ SMS │ │ Push │
│Service│ │Service│ │Service│
└──┬───┘ └──┬───┘ └──┬───┘
   │        │        │
   │        │        │
   ▼        ▼        ▼
┌─────────────────────────────┐
│      PostgreSQL            │
│  - notifications table     │
│  - notification_attempts   │
└─────────────────────────────┘
```

### Queue Flow Diagram

```
Gateway Service
    │
    ├─► notifications.queue (main queue)
    │
    ├─► email.queue ──► EmailService ──► Mock SMTP Provider
    │
    ├─► sms.queue ────► SMSService ────► Mock Twilio Provider
    │
    └─► push.queue ───► PushService ───► Mock FCM Provider
```

### Technology Stack

- **.NET 8** - Latest C# framework
- **RabbitMQ** - Message broker for asynchronous communication
- **PostgreSQL** - Relational database with EF Core
- **Docker & Docker Compose** - Containerization and orchestration
- **Prometheus** - Metrics collection
- **Grafana** - Metrics visualization
- **Elasticsearch** - Log storage
- **Kibana** - Log visualization
- **Serilog** - Structured logging
- **Polly** - Retry and circuit breaker policies
- **AutoMapper** - Object mapping
- **FluentValidation** - Request validation
- **Swagger/OpenAPI** - API documentation

## 📁 Project Structure

```
.
├── src/
│   ├── GatewayService/          # API Gateway
│   │   ├── Controllers/         # REST API controllers
│   │   ├── Validators/          # FluentValidation validators
│   │   ├── Mapping/             # AutoMapper profiles
│   │   └── Program.cs           # Service configuration
│   │
│   ├── EmailService/            # Email microservice
│   │   ├── Services/            # Business logic
│   │   ├── Providers/           # Email provider (Mock SMTP)
│   │   └── Program.cs           # Service configuration
│   │
│   ├── SMSService/              # SMS microservice
│   │   ├── Services/            # Business logic
│   │   ├── Providers/          # SMS provider (Mock Twilio)
│   │   └── Program.cs          # Service configuration
│   │
│   ├── PushService/            # Push notification microservice
│   │   ├── Services/           # Business logic
│   │   ├── Providers/         # Push provider (Mock FCM)
│   │   └── Program.cs         # Service configuration
│   │
│   └── Shared/                 # Shared library
│       ├── Models/             # Domain models
│       ├── DTOs/               # Data transfer objects
│       ├── Messaging/          # RabbitMQ interfaces
│       └── Database/           # EF Core context & repository
│
├── infrastructure/
│   ├── docker-compose.yml      # Docker orchestration
│   ├── prometheus/
│   │   └── prometheus.yml      # Prometheus configuration
│   └── grafana/
│       ├── provisioning/      # Grafana datasources
│       └── dashboards/        # Pre-configured dashboards
│
└── README.md                   # This file
```

## 🚀 Quick Start

### Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose)
- .NET 8 SDK (for local development)
- Git

### Running with Docker Compose

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Development-of-a-distributed-service-UDV
   ```

2. **Start all services**
   ```bash
   cd infrastructure
   docker-compose up -d
   ```

3. **Verify services are running**
   ```bash
   docker-compose ps
   ```

4. **Access the services**
   - **Gateway API**: http://localhost:5000
   - **Swagger UI**: http://localhost:5000/swagger
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)
   - **Prometheus**: http://localhost:9090
   - **Grafana**: http://localhost:3000 (admin/admin)
   - **Kibana**: http://localhost:5601
   - **Elasticsearch**: http://localhost:9200

### Health Checks

- Gateway Service: http://localhost:5000/health
- Email Service: http://localhost:5001/health
- SMS Service: http://localhost:5002/health
- Push Service: http://localhost:5003/health

### Metrics Endpoints

- Gateway Service: http://localhost:5000/metrics
- Email Service: http://localhost:5001/metrics
- SMS Service: http://localhost:5002/metrics
- Push Service: http://localhost:5003/metrics

## 📡 API Documentation

### Send Notification

**POST** `/api/notifications/send`

Send a notification through the specified channel.

**Request Body:**
```json
{
  "channel": "email",
  "recipient": "user@example.com",
  "message": "Hello, this is a test notification!",
  "metadata": {
    "subject": "Test Email",
    "priority": "high"
  }
}
```

**Response:**
```json
{
  "notificationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Supported Channels:**
- `email` - Email notifications
- `sms` - SMS notifications
- `push` - Push notifications

### Get Notification Status

**GET** `/api/notifications/{id}`

Retrieve the status and details of a notification.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": "email",
  "recipient": "user@example.com",
  "message": "Hello, this is a test notification!",
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

## 🔧 Configuration

### Environment Variables

**Gateway Service:**
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `RabbitMQ__ConnectionString` - RabbitMQ connection string
- `Elasticsearch__Uri` - Elasticsearch URI

**Channel Services (Email/SMS/Push):**
- Same as Gateway Service

### Database Schema

**notifications table:**
- `id` (UUID, Primary Key)
- `channel` (VARCHAR(50))
- `recipient` (VARCHAR(500))
- `message` (TEXT)
- `status` (VARCHAR(50))
- `retries` (INT)
- `errors` (TEXT, nullable)
- `metadata` (TEXT, nullable)
- `createdAt` (TIMESTAMP)
- `updatedAt` (TIMESTAMP, nullable)

**notification_attempts table:**
- `id` (UUID, Primary Key)
- `notificationId` (UUID, Foreign Key)
- `attemptedAt` (TIMESTAMP)
- `status` (VARCHAR(50))
- `errorMessage` (TEXT, nullable)
- `retryNumber` (INT)

## 🔄 Retry Mechanism

Each channel service implements retry logic with exponential backoff:

- **Max Retries**: 3
- **Backoff Strategy**: Exponential (2^retryAttempt seconds)
- **Retry Conditions**: All exceptions are retried
- **Dead Letter Queue**: Failed messages after max retries are sent to DLX

## 📊 Monitoring & Observability

### Prometheus Metrics

Each service exposes the following metrics:

- `{service}_notifications_processed_total` - Counter of processed notifications
- `{service}_notifications_failed_total` - Counter of failed notifications
- `{service}_notification_processing_duration_seconds` - Histogram of processing time

### Grafana Dashboards

Pre-configured dashboards are available in `infrastructure/grafana/dashboards/`:
- Notification System Overview
- Service Health Metrics
- Queue Metrics
- Error Rates

### Logging

All services use Serilog with:
- **Console Sink** - For local development
- **Elasticsearch Sink** - For centralized logging
- **Structured Logging** - JSON format with correlation IDs

View logs in Kibana: http://localhost:5601

## 🧪 Testing

### Manual Testing with cURL

**Send Email Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "email",
    "recipient": "test@example.com",
    "message": "Test email notification"
  }'
```

**Send SMS Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "sms",
    "recipient": "+1234567890",
    "message": "Test SMS notification"
  }'
```

**Send Push Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "push",
    "recipient": "device-token-123",
    "message": "Test push notification"
  }'
```

**Get Notification Status:**
```bash
curl http://localhost:5000/api/notifications/{notification-id}
```

## 🔌 Extending with New Channels

To add a new channel (e.g., WhatsApp, Telegram, Slack):

1. **Create a new microservice** following the pattern of EmailService
2. **Add provider interface** in `Providers/` folder
3. **Implement mock/provider** for the channel
4. **Add queue name** in `Shared/Messaging/QueueNames.cs`
5. **Update GatewayService** to route to the new queue
6. **Add service to docker-compose.yml**
7. **Update Prometheus configuration** to scrape new service

Example structure:
```
src/
└── WhatsAppService/
    ├── Services/
    │   ├── IWhatsAppService.cs
    │   ├── WhatsAppService.cs
    │   └── WhatsAppWorker.cs
    ├── Providers/
    │   ├── IWhatsAppProvider.cs
    │   └── MockWhatsAppProvider.cs
    └── Program.cs
```

## 🏥 Health Checks

All services expose health check endpoints:

- `/health` - Overall health status
- `/health/ready` - Readiness probe (checks dependencies)
- `/health/live` - Liveness probe

Health checks verify:
- PostgreSQL connectivity
- RabbitMQ connectivity
- Service availability

## 🔒 Security Considerations

For production deployment:

1. **Change default passwords** (PostgreSQL, RabbitMQ, Grafana)
2. **Use secrets management** (Azure Key Vault, AWS Secrets Manager)
3. **Enable TLS/SSL** for all services
4. **Implement authentication** for API endpoints
5. **Use network policies** to restrict service communication
6. **Enable Elasticsearch security** features
7. **Implement rate limiting** on Gateway Service

## 📈 Performance Tuning

### RabbitMQ
- Adjust prefetch count based on message processing time
- Configure queue TTL for message retention
- Set up queue mirroring for high availability

### PostgreSQL
- Create indexes on frequently queried columns
- Configure connection pooling
- Set up read replicas for scaling

### Services
- Adjust retry policies based on provider SLAs
- Configure concurrent message processing
- Scale services horizontally based on load

## 🐛 Troubleshooting

### Services not starting
- Check Docker logs: `docker-compose logs <service-name>`
- Verify all dependencies are healthy
- Check port conflicts

### Messages not processing
- Verify RabbitMQ queues exist: http://localhost:15672
- Check service logs for errors
- Verify database connectivity

### Metrics not appearing
- Check Prometheus targets: http://localhost:9090/targets
- Verify service metrics endpoints are accessible
- Check Prometheus configuration

## 📝 License

This project is provided as-is for educational and demonstration purposes.

## 🤝 Contributing

This is a demonstration project. For production use, consider:
- Adding comprehensive unit and integration tests
- Implementing proper error handling and recovery
- Adding API authentication and authorization
- Setting up CI/CD pipelines
- Adding performance benchmarks

## 📚 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [RabbitMQ Best Practices](https://www.rabbitmq.com/best-practices.html)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Serilog Documentation](https://serilog.net/)

---

**Project generation completed.**

