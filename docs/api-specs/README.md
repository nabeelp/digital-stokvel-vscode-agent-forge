# API Specifications

This directory contains OpenAPI (Swagger) specifications for all Digital Stokvel Banking APIs.

## APIs

### Core Banking API

**Base URL**: `https://api.digitalstokvel.co.za/api`

#### Groups API
- `POST /groups` - Create stokvel group
- `GET /groups/{id}` - Get group details
- `PUT /groups/{id}` - Update group
- `GET /groups/{id}/ledger` - Get immutable ledger
- `GET /groups/{id}/export` - Export ledger as PDF

#### Members API
- `POST /groups/{id}/members` - Invite member
- `GET /groups/{id}/members` - List members
- `DELETE /groups/{id}/members/{memberId}` - Remove member

#### Contributions API
- `POST /contributions` - Make contribution
- `GET /contributions/{id}` - Get contribution receipt
- `GET /groups/{id}/contributions` - List group contributions

#### Payouts API
- `POST /payouts` - Initiate payout (Chairperson)
- `POST /payouts/{id}/approve` - Approve payout (Treasurer)
- `GET /payouts/{id}` - Get payout details

### USSD API

**Endpoint**: `POST /api/ussd/session`

USSD session management for feature phone interactions.

## OpenAPI Specs

Full OpenAPI 3.0 specifications will be generated from ASP.NET Core controllers using Swashbuckle.

Access at: `https://localhost:7001/swagger` (dev) or `https://api.digitalstokvel.co.za/swagger` (prod)

## Authentication

All API endpoints (except USSD) require JWT Bearer token authentication:

```
Authorization: Bearer <token>
```

USSD endpoints use MNO signature validation.

## License

Proprietary - All rights reserved.
