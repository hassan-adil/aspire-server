# .NET 9 Infrastructure Blueprint

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Architecture](https://img.shields.io/badge/Architecture-Clean-blue)
![License](https://img.shields.io/badge/License-MIT-green)

An **architecture-first microservice foundation** built with .NET 9.

This project emphasizes **infrastructure, scalability, and clean architecture** rather than business domain logic.  
It is a **production-ready blueprint** for building distributed, multi-tenant microservices.

---

## 🎯 Purpose

- Reusable enterprise-grade microservice baseline
- Transactional **Outbox pattern**
- Multi-tenant awareness
- Identity integration (Keycloak)
- Deterministic structured seeding
- Observability-first logging (including DB query snapshots and latency)

No business logic is included — purely infrastructure focus.

---

## 🏗 Architectural Highlights

### Clean Architecture
- API Layer
- Application Layer
- Domain Layer
- Infrastructure Layer
- Contracts & Shared Kernel

Strict dependency direction and layer separation are enforced.

### Event-Driven Ready
- RabbitMQ integration
- Reliable message publishing
- Consumer extensibility
- Outbox pattern for transactional consistency

### Observability & Logging
- Middleware-based centralized request logging
- Structured JSON logs
- **Database query logging**
- **Query latency tracking**
- Snapshot storage for DB commands
- Identifies slow queries and bottlenecks

### Multi-Tenant Awareness
- Tenant resolution per request
- Tenant-aware DbContext
- Isolation-ready architecture for SaaS scenarios

### Identity & Security
- Keycloak (OIDC / OAuth2)
- JWT authentication
- Role based authorization control (RBAC)

### Structured Seeding Mechanism
- Deterministic & idempotent seeding
- Environment-aware
- CI/CD ready
- Supports tenant-specific seeds

### Infrastructure & DevOps Ready
- Docker support
- .NET Aspire compatible
- Environment-based configuration
- Production-oriented defaults

---

## 🧱 Technology Stack

- .NET 9
- ASP.NET Core
- RabbitMQ
- PostgreSQL
- Keycloak
- Docker
- .NET Aspire
- MassTransit

---

## 🔒 Design Principles

- SOLID principles
- Explicit architectural boundaries
- Dependency inversion
- Transactional integrity
- Observability-first mindset
- Multi-tenant scalability
- Production-grade defaults

---

## 🚀 Usage

- Microservice starter template
- Distributed system baseline
- SaaS-ready infrastructure blueprint
- Architecture reference project

---

## 📌 Roadmap - Future Enhancements

- OpenTelemetry integration
- Centralized log aggregation (Elastic Stack)
- Distributed tracing
- Saga orchestration
- CQRS layer

---

## 📄 License

MIT License

---

## 👨‍💻 Author

Muhammad Hassan Adil