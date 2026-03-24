# Architecture Documentation

This directory contains architecture diagrams, design documents, and technical specifications for the Digital Stokvel Banking platform.

## Contents

### High-Level Architecture
- System context diagram
- Container diagram (C4 model)
- Component diagrams
- Data flow diagrams

### Integration Patterns
- USSD gateway integration
- MNO API integration patterns
- Payment processing flows
- Notification delivery architecture

### Security Architecture
- Authentication and authorization flows
- Data encryption at rest and in transit
- POPIA compliance architecture
- FICA regulatory compliance

### Database Schema
- Entity-relationship diagrams
- Table definitions
- Index strategies
- Migration patterns

## Placeholder for Diagrams

```mermaid
graph TB
    subgraph "Mobile & Web"
        Mobile[Mobile App<br/>React Native]
        Web[Chairperson Dashboard<br/>React + Vite]
        USSD[USSD Gateway<br/>*120*STOKVEL#]
    end
    
    subgraph "Azure South Africa North"
        API[Container Apps<br/>ASP.NET Core API]
        DB[(PostgreSQL<br/>Flexible Server)]
        KV[Key Vault<br/>Secrets]
        SB[Service Bus<br/>Async Processing]
        Blob[Blob Storage<br/>Ledger Exports]
        APIM[API Management<br/>USSD Routing]
        AppInsights[Application Insights<br/>Monitoring]
    end
    
    Mobile --> API
    Web --> API
    USSD --> APIM
    APIM --> API
    API --> DB
    API --> KV
    API --> SB
    API --> Blob
    API --> AppInsights
```

## License

Proprietary - All rights reserved.
