# Quick Start Guide

## Prerequisites

Before running the project, ensure you have:

1. **Docker Desktop** installed and running
   - Download from: https://www.docker.com/products/docker-desktop
   - Make sure Docker is running (you should see the Docker icon in your system tray)

2. **Git** (optional, if cloning from repository)

## Step-by-Step Instructions

### Step 1: Navigate to the Infrastructure Directory

Open a terminal/command prompt and navigate to the infrastructure folder:

```bash
cd infrastructure
```

### Step 2: Start All Services

Run Docker Compose to start all services:

```bash
docker-compose up -d
```

The `-d` flag runs containers in detached mode (in the background).

**First time running?** This will:
- Download all required Docker images (may take a few minutes)
- Build the .NET services
- Start all containers

### Step 3: Verify Services are Running

Check the status of all containers:

```bash
docker-compose ps
```

You should see all services with status "Up" or "Up (healthy)".

**Expected services:**
- notification-postgres
- notification-rabbitmq
- notification-elasticsearch
- notification-kibana
- notification-prometheus
- notification-grafana
- notification-gateway
- notification-email
- notification-sms
- notification-push

### Step 4: Check Service Logs (Optional)

If you want to see what's happening, check the logs:

```bash
# View all logs
docker-compose logs -f

# View logs for a specific service
docker-compose logs -f gateway-service
docker-compose logs -f email-service
```

Press `Ctrl+C` to exit log viewing.

### Step 5: Wait for Services to Initialize

Wait about 30-60 seconds for all services to fully start and initialize:
- Database migrations will run automatically
- RabbitMQ queues will be created
- Services will connect to dependencies

### Step 6: Access the Services

Once all services are running, you can access:

#### API Gateway & Documentation
- **Swagger UI**: http://localhost:5000/swagger
- **API Base URL**: http://localhost:5000/api/notifications
- **Health Check**: http://localhost:5000/health
- **Metrics**: http://localhost:5000/metrics

#### Monitoring & Management
- **RabbitMQ Management**: http://localhost:15672
  - Username: `guest`
  - Password: `guest`
  
- **Prometheus**: http://localhost:9090

- **Grafana**: http://localhost:3000
  - Username: `admin`
  - Password: `admin`
  - (You'll be prompted to change password on first login)

- **Kibana**: http://localhost:5601

- **Elasticsearch**: http://localhost:9200

#### Channel Service Health Checks
- Email Service: http://localhost:5001/health
- SMS Service: http://localhost:5002/health
- Push Service: http://localhost:5003/health

## Testing the API

### Option 1: Using Swagger UI (Easiest)

1. Open http://localhost:5000/swagger in your browser
2. Expand the `POST /api/notifications/send` endpoint
3. Click "Try it out"
4. Enter a test request:

```json
{
  "channel": "email",
  "recipient": "test@example.com",
  "message": "Hello from the notification system!",
  "metadata": {
    "subject": "Test Notification"
  }
}
```

5. Click "Execute"
6. Copy the `notificationId` from the response
7. Use the `GET /api/notifications/{id}` endpoint to check the status

### Option 2: Using cURL

**Send an Email Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d "{
    \"channel\": \"email\",
    \"recipient\": \"test@example.com\",
    \"message\": \"Hello, this is a test email notification!\"
  }"
```

**Send an SMS Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d "{
    \"channel\": \"sms\",
    \"recipient\": \"+1234567890\",
    \"message\": \"Hello, this is a test SMS notification!\"
  }"
```

**Send a Push Notification:**
```bash
curl -X POST http://localhost:5000/api/notifications/send \
  -H "Content-Type: application/json" \
  -d "{
    \"channel\": \"push\",
    \"recipient\": \"device-token-123\",
    \"message\": \"Hello, this is a test push notification!\"
  }"
```

**Get Notification Status:**
```bash
# Replace {notification-id} with the ID from the send response
curl http://localhost:5000/api/notifications/{notification-id}
```

### Option 3: Using PowerShell (Windows)

**Send Notification:**
```powershell
$body = @{
    channel = "email"
    recipient = "test@example.com"
    message = "Hello from PowerShell!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/send" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

**Get Status:**
```powershell
$notificationId = "your-notification-id-here"
Invoke-RestMethod -Uri "http://localhost:5000/api/notifications/$notificationId"
```

## Viewing Logs

### In Elasticsearch/Kibana

1. Open Kibana: http://localhost:5601
2. Go to "Discover" (left sidebar)
3. Create an index pattern: `notification-*-*` (with wildcard)
4. View logs from all services

### In Docker

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f gateway-service
docker-compose logs -f email-service
docker-compose logs -f sms-service
docker-compose logs -f push-service
```

## Viewing Metrics

1. Open Grafana: http://localhost:3000
2. Login with admin/admin
3. Navigate to "Dashboards" → "Import"
4. The pre-configured dashboard should be available
5. Or explore metrics in Prometheus: http://localhost:9090

## Stopping the Services

To stop all services:

```bash
cd infrastructure
docker-compose down
```

To stop and remove all data (volumes):

```bash
docker-compose down -v
```

**Warning:** This will delete all database data, logs, and metrics!

## Troubleshooting

### Services won't start

1. **Check Docker is running:**
   ```bash
   docker ps
   ```

2. **Check for port conflicts:**
   - Make sure ports 5000, 5001, 5002, 5003, 5432, 5672, 15672, 9090, 3000, 5601, 9200 are not in use

3. **View service logs:**
   ```bash
   docker-compose logs <service-name>
   ```

4. **Restart a specific service:**
   ```bash
   docker-compose restart <service-name>
   ```

### Database connection errors

- Wait a bit longer for PostgreSQL to fully initialize
- Check PostgreSQL is healthy: `docker-compose ps postgres`

### RabbitMQ connection errors

- Check RabbitMQ is healthy: `docker-compose ps rabbitmq`
- Access RabbitMQ Management UI to verify queues exist

### Services show as unhealthy

- Check individual service logs
- Verify dependencies (PostgreSQL, RabbitMQ) are running
- Wait a bit longer for services to connect

### Can't access services

- Verify Docker containers are running: `docker-compose ps`
- Check if ports are already in use
- Try accessing via `localhost` instead of `127.0.0.1`

## Next Steps

- Explore the Swagger documentation at http://localhost:5000/swagger
- Check RabbitMQ queues at http://localhost:15672
- View metrics in Grafana at http://localhost:3000
- Explore logs in Kibana at http://localhost:5601
- Read the full README.md for architecture details

## Need Help?

- Check service logs: `docker-compose logs -f`
- Verify all containers are running: `docker-compose ps`
- Check the main README.md for detailed documentation

