# About Project

TR Backend is a social media management platform backend designed to support content creation, post management and social account integrations.

The platform allows users to:

- Manage social media content in one place  
- Create and edit posts  
- Generate post content using Gemini AI integration  
- Store and manage media assets with MinIO  
- Use Redis for caching and performance optimization  
- Connect social media accounts *(toDo)*

The goal of the project is to provide backend infrastructure for an AI-powered social media management system.

---
## .env file

- GEMINI_API_KEY=YOUR GEMINI API KEY
- LINKEDIN_CLIENT_ID=YOUR LINKEDIN CLIENT ID
- LINKEDIN_CLIENT_SECRET=YOUR LINKEDIN SECRET 
- LINKEDIN_REDIRECT_URI=LINKED IN REDIRECT URI

---

## Core Features

### Content Management

- Create posts
- Edit post content
- Manage publication-ready content
- Store media assets

### AI Content Generation

- Gemini API integration
- Generate social media post ideas
- AI-assisted content creation
- Support for automated content workflows

### Social Media Management

- Centralized social media management
- Multi-account support
- Social account connection

## Overview

TR Backend is a REST API structured using a multi-project architecture with separated business logic, repositories, domain models and tests.

## Architecture

```text
Client
 ↓
tr-api
 ↓
tr-service
 ↓
tr-repository
 ↓
PostgreSQL

Additional services:
- Redis (caching)
- MinIO (object storage)
```

## Tech Stack

- ASP.NET Core
- C#
- PostgreSQL
- Redis
- MinIO
- Docker Compose
- xUnit / NUnit
- Entity Framework Core *(if used)*

## Solution Structure

```text
tr-backend/
│
├── tr-api/
├── tr-service/
├── tr-repository/
├── tr-core/
├── Tests/
└── docker-compose.yml
```

## Features

- Cookie-based authentication
- Session management
- REST API
- Layered Architecture
- Repository Pattern
- Service Layer
- PostgreSQL persistence
- Redis caching
- MinIO object storage
- Dockerized dependencies
- Unit/Integration tests

---

## Live Deployment

Application is deployed and available on Azure:

**API Base URL**

```http
https://team-red-api.azurewebsites.net/
```

Hosted on Microsoft Azure App Service.

### Production Environment

- Azure App Service
- PostgreSQL
- Redis
- MinIO
- Docker-based local development
- Cloud deployed backend
- Microsoft Azure App Service


# Getting Started

## Clone Repository

```bash
git clone https://github.com/norbik2004/tr-backend.git
cd tr-backend
```

## Start Infrastructure

Run dependencies:

```bash
docker compose up -d
```

Starts:

- PostgreSQL → localhost:5435
- Redis → localhost:6379
- MinIO API → localhost:9000
- MinIO Console → http://localhost:9001

### MinIO Credentials

```text
Login: miniouser
Password: miniopassword
```

---

## Restore

```bash
dotnet restore
```

## Build

```bash
dotnet build
```

## Run API

```bash
cd tr-api
dotnet run
```

---

## Configuration

Example `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5435;Database=postgres;Username=postgres;Password=postgres"
  },

  "Redis": {
    "ConnectionString": "localhost:6379"
  },

  "Minio": {
    "Endpoint": "localhost:9000",
    "AccessKey": "miniouser",
    "SecretKey": "miniopassword"
  }
}
```

---

## Docker Services

### PostgreSQL

```yaml
Port: 5435
```

### Redis

```yaml
Port: 6379
```

### MinIO

```yaml
Ports:
9000
9001
```

---

## Run Tests

```bash
dotnet test
```


## Future Improvements

- CI/CD pipeline
- Background jobs
- Unit tests
- Linking social media accounts

## License

This project is for educational and portfolio purposes.

![https___dev-to-uploads s3 amazonaws com_uploads_articles_2amdy7l8ttylg1qmh6qh](https://github.com/user-attachments/assets/94e80ec2-4679-4f9a-82f9-f4636b77a97c)
