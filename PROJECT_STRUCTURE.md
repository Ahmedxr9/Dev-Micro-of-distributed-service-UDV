# 📁 Project Structure

## Complete Project Structure for Distributed Notification System

```
Development-of-a-distributed-service-UDV/
│
├── 📄 README.md                          # Main project documentation
├── 📄 QUICKSTART.md                      # Quick start guide
├── 📄 TESTING_GUIDE.md                   # Testing documentation
├── 📄 PROJECT_STRUCTURE.md               # This file
├── 📄 FIXES_APPLIED.md                   # Applied fixes log
│
├── 📁 src/                               # Source code
│   │
│   ├── 📁 GatewayService/               # API Gateway (Entry Point)
│   │   ├── 📄 Program.cs                 # Application entry point
│   │   ├── 📄 GatewayWebApplicationService.cs  # Service configuration
│   │   ├── 📄 GatewayService.csproj      # Project file
│   │   ├── 📄 Dockerfile                 # Docker configuration
│   │   ├── 📄 appsettings.json           # Configuration
│   │   ├── 📄 appsettings.Development.json
│   │   │
│   │   ├── 📁 Controllers/              # REST API Controllers
│   │   │   └── 📄 NotificationsController.cs  # Main API controller
│   │   │
│   │   ├── 📁 Validators/               # FluentValidation
│   │   │   └── 📄 NotificationRequestValidator.cs
│   │   │
│   │   └── 📁 Mapping/                  # AutoMapper
│   │       └── 📄 MappingProfile.cs
│   │
│   ├── 📁 EmailService/                 # Email Microservice
│   │   ├── 📄 Program.cs                # Service entry point
│   │   ├── 📄 EmailService.csproj
│   │   ├── 📄 Dockerfile
│   │   ├── 📄 appsettings.json
│   │   │
│   │   ├── 📁 Services/                 # Business Logic
│   │   │   ├── 📄 IEmailService.cs      # Service interface
│   │   │   ├── 📄 EmailService.cs       # Service implementation
│   │   │   └── 📄 EmailWorker.cs        # Background worker
│   │   │
│   │   └── 📁 Providers/                # Email Providers
│   │       ├── 📄 IEmailProvider.cs      # Provider interface
│   │       └── 📄 MockEmailProvider.cs   # Mock implementation
│   │
│   ├── 📁 SMSService/                   # SMS Microservice
│   │   ├── 📄 Program.cs
│   │   ├── 📄 SMSService.csproj
│   │   ├── 📄 Dockerfile
│   │   ├── 📄 appsettings.json
│   │   │
│   │   ├── 📁 Services/
│   │   │   ├── 📄 ISMSService.cs
│   │   │   ├── 📄 SMSService.cs
│   │   │   └── 📄 SMSWorker.cs
│   │   │
│   │   └── 📁 Providers/
│   │       ├── 📄 ISMSProvider.cs
│   │       └── 📄 MockSMSProvider.cs
│   │
│   ├── 📁 PushService/                  # Push Notification Microservice
│   │   ├── 📄 Program.cs
│   │   ├── 📄 PushService.csproj
│   │   ├── 📄 Dockerfile
│   │   ├── 📄 appsettings.json
│   │   │
│   │   ├── 📁 Services/
│   │   │   ├── 📄 IPushService.cs
│   │   │   ├── 📄 PushService.cs
│   │   │   └── 📄 PushWorker.cs
│   │   │
│   │   └── 📁 Providers/
│   │       ├── 📄 IPushProvider.cs
│   │       └── 📄 MockPushProvider.cs
│   │
│   └── 📁 Shared/                       # Shared Library (Common Code)
│       ├── 📄 Shared.csproj
│       │
│       ├── 📁 Models/                   # Domain Models
│       │   └── 📄 Notification.cs       # Notification entity
│       │
│       ├── 📁 DTOs/                     # Data Transfer Objects
│       │   ├── 📄 NotificationRequestDto.cs
│       │   ├── 📄 NotificationResponseDto.cs
│       │   └── 📄 NotificationStatusDto.cs
│       │
│       ├── 📁 Database/                 # Database Layer
│       │   ├── 📄 NotificationDbContext.cs      # EF Core context
│       │   ├── 📄 INotificationRepository.cs     # Repository interface
│       │   └── 📄 NotificationRepository.cs     # Repository implementation
│       │
│       ├── 📁 Messaging/                # RabbitMQ Messaging
│       │   ├── 📄 IMessageProducer.cs   # Producer interface
│       │   ├── 📄 IMessageConsumer.cs  # Consumer interface
│       │   ├── 📄 RabbitMQProducer.cs   # RabbitMQ producer
│       │   ├── 📄 RabbitMQConsumer.cs   # RabbitMQ consumer
│       │   ├── 📄 NotificationMessage.cs        # Message model
│       │   └── 📄 QueueNames.cs         # Queue name constants
│       │
│       └── 📁 WebApplication/           # Web Application Infrastructure
│           ├── 📄 IWebApplicationService.cs      # Service interface
│           ├── 📄 BaseWebApplicationService.cs   # Base implementation
│           └── 📄 WebApplicationConfiguration.cs  # Configuration helpers
│
├── 📁 infrastructure/                   # Infrastructure & DevOps
│   ├── 📄 docker-compose.yml            # Docker Compose configuration
│   │
│   ├── 📁 prometheus/                   # Prometheus Configuration
│   │   └── 📄 prometheus.yml            # Metrics collection config
│   │
│   └── 📁 grafana/                      # Grafana Configuration
│       ├── 📁 provisioning/
│       │   ├── 📁 datasources/
│       │   │   └── 📄 prometheus.yml   # Prometheus datasource
│       │   └── 📁 dashboards/
│       │       └── 📄 dashboard.yml     # Dashboard provisioning
│       └── 📁 dashboards/
│           └── 📄 notification-system.json  # Dashboard definition
│
└── 📁 Scripts/                          # Utility Scripts
    ├── 📄 test-system.ps1               # Automated test script
    ├── 📄 test-get-example.ps1          # GET endpoint example
    ├── 📄 delete-rabbitmq-queues.ps1    # Queue cleanup script
    └── 📄 test-requests.json            # Test request examples
```

---

## 📋 Component Descriptions

### 🚪 GatewayService
**Purpose:** API Gateway - Entry point for all client requests

**Key Components:**
- `NotificationsController.cs` - REST API endpoints
  - `POST /api/notifications/send` - Send notification
  - `GET /api/notifications/{id}` - Get notification status
- `NotificationRequestValidator.cs` - Request validation
- `MappingProfile.cs` - DTO to Entity mapping
- `GatewayWebApplicationService.cs` - Service configuration using interface pattern

**Port:** 5000

---

### 📧 EmailService
**Purpose:** Processes email notifications from RabbitMQ queue

**Key Components:**
- `EmailWorker.cs` - Background worker that consumes messages
- `EmailService.cs` - Business logic for email processing
- `MockEmailProvider.cs` - Mock email provider (for testing)

**Port:** 5001

---

### 📱 SMSService
**Purpose:** Processes SMS notifications from RabbitMQ queue

**Key Components:**
- `SMSWorker.cs` - Background worker
- `SMSService.cs` - Business logic
- `MockSMSProvider.cs` - Mock SMS provider

**Port:** 5002

---

### 🔔 PushService
**Purpose:** Processes push notifications from RabbitMQ queue

**Key Components:**
- `PushWorker.cs` - Background worker
- `PushService.cs` - Business logic
- `MockPushProvider.cs` - Mock push provider

**Port:** 5003

---

### 🔗 Shared Library
**Purpose:** Common code shared across all services

**Key Components:**

#### Models/
- `Notification.cs` - Database entity model

#### DTOs/
- `NotificationRequestDto.cs` - Request DTO
- `NotificationResponseDto.cs` - Response DTO
- `NotificationStatusDto.cs` - Status DTO

#### Database/
- `NotificationDbContext.cs` - EF Core DbContext
- `NotificationRepository.cs` - Data access layer

#### Messaging/
- `RabbitMQProducer.cs` - Message publisher
- `RabbitMQConsumer.cs` - Message consumer
- `QueueNames.cs` - Queue name constants

#### WebApplication/
- `IWebApplicationService.cs` - Service interface
- `BaseWebApplicationService.cs` - Base implementation
- `WebApplicationConfiguration.cs` - Common configuration helpers

---

### 🐳 Infrastructure
**Purpose:** Docker and monitoring configuration

**Components:**
- `docker-compose.yml` - Orchestrates all services
- `prometheus/` - Metrics collection
- `grafana/` - Metrics visualization

---

## 🔄 Data Flow

```
Client Request
    ↓
GatewayService (Port 5000)
    ├── Validates request
    ├── Stores in PostgreSQL
    └── Publishes to RabbitMQ
         ↓
    RabbitMQ Queues
         ├── email.queue → EmailService (Port 5001)
         ├── sms.queue → SMSService (Port 5002)
         └── push.queue → PushService (Port 5003)
              ↓
         Channel Services
              ├── Process notification
              ├── Update status in PostgreSQL
              └── Log to Elasticsearch
```

---

## 🗄️ Database Schema

**Tables:**
- `notifications` - Main notification records
- `notification_attempts` - Processing attempt history

---

## 🔌 External Services

1. **PostgreSQL** (Port 5432) - Database
2. **RabbitMQ** (Port 5672) - Message broker
3. **RabbitMQ Management** (Port 15672) - Management UI
4. **Elasticsearch** (Port 9200) - Log storage
5. **Kibana** (Port 5601) - Log visualization
6. **Prometheus** (Port 9090) - Metrics collection
7. **Grafana** (Port 3000) - Metrics visualization

---

## 📦 Technology Stack

- **.NET 8** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **RabbitMQ** - Message broker
- **Docker** - Containerization
- **Prometheus** - Metrics
- **Grafana** - Visualization
- **Serilog** - Logging
- **Elasticsearch** - Log storage
- **FluentValidation** - Validation
- **AutoMapper** - Object mapping

---

## 🎯 Key Features

1. **Microservices Architecture** - Separate services for each channel
2. **Message Queue** - Asynchronous processing via RabbitMQ
3. **Retry Logic** - Automatic retry with exponential backoff
4. **Health Checks** - Built-in health monitoring
5. **Metrics** - Prometheus metrics collection
6. **Logging** - Centralized logging with Serilog/Elasticsearch
7. **Interface-Based Design** - Reusable WebApplication interface
8. **Validation** - Request validation with FluentValidation
9. **Docker Support** - Full containerization
10. **Swagger UI** - API documentation

---

## 📝 File Naming Conventions

- **Controllers:** `*Controller.cs`
- **Services:** `*Service.cs` (implementation), `I*Service.cs` (interface)
- **Workers:** `*Worker.cs`
- **Providers:** `*Provider.cs` (implementation), `I*Provider.cs` (interface)
- **DTOs:** `*Dto.cs`
- **Validators:** `*Validator.cs`
- **Configuration:** `appsettings.json`, `appsettings.Development.json`

---

## 🚀 Quick Navigation

- **Start Services:** `cd infrastructure && docker-compose up -d`
- **Run Gateway:** `cd src/GatewayService && dotnet run`
- **Test API:** `http://localhost:5000/swagger`
- **View Logs:** `http://localhost:5601` (Kibana)
- **View Metrics:** `http://localhost:3000` (Grafana)
- **RabbitMQ UI:** `http://localhost:15672` (guest/guest)

---

**Last Updated:** 2024-12-12

