# Expense Management System - Azure Architecture

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         EXPENSE MANAGEMENT SYSTEM                            │
│                        Modern Cloud-Native Solution                          │
└─────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────────┐
│  EXTERNAL ACCESS                                                           │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  👤 End Users  ──────────> HTTPS ──────────> Internet                     │
│  👤 Managers                                                               │
│                                                                            │
└───────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │
                                    ▼
┌───────────────────────────────────────────────────────────────────────────┐
│  AZURE APP SERVICE (Linux, .NET 8)                                        │
│  ┌──────────────────────────────────────────────────────────────────────┐ │
│  │  📱 Razor Pages UI                                                    │ │
│  │    - Index (List Expenses)                                           │ │
│  │    - Add Expense                                                     │ │
│  │    - Edit Expense                                                    │ │
│  │    - Approve Expenses (Manager)                                      │ │
│  │    - Chat UI (AI Assistant)                                          │ │
│  ├──────────────────────────────────────────────────────────────────────┤ │
│  │  🔌 REST APIs                                                         │ │
│  │    - /api/expenses (GET, POST, PUT, DELETE)                         │ │
│  │    - /api/expenses/{id}                                             │ │
│  │    - /api/expenses/filter                                           │ │
│  │    - /api/expenses/{id}/submit                                      │ │
│  │    - /api/expenses/{id}/approve                                     │ │
│  │    - /api/expenses/{id}/reject                                      │ │
│  │    - /swagger (API Documentation)                                   │ │
│  └──────────────────────────────────────────────────────────────────────┘ │
│                                                                            │
│  🔐 Authentication: User-Assigned Managed Identity                        │
│      Client ID: [mid-expensemgmt-xxxxxx]                                 │
└───────────────────────────────────────────────────────────────────────────┘
        │                                         │
        │                                         │
        │ Managed Identity                        │ Managed Identity
        │ Authentication                          │ Authentication
        │                                         │
        ▼                                         ▼
┌─────────────────────────────┐     ┌───────────────────────────────────┐
│  AZURE SQL DATABASE         │     │  AZURE OPENAI SERVICE             │
│  ┌────────────────────────┐ │     │  ┌──────────────────────────────┐ │
│  │  📊 Tables:            │ │     │  │  🤖 GPT-4o Model             │ │
│  │    - Expenses          │ │     │  │     (swedencentral)          │ │
│  │    - Users             │ │     │  │                              │ │
│  │    - ExpenseCategories │ │     │  │  Features:                   │ │
│  │    - ExpenseStatus     │ │     │  │    - Function Calling        │ │
│  │    - Roles             │ │     │  │    - Natural Language Query  │ │
│  ├────────────────────────┤ │     │  │    - Context-Aware           │ │
│  │  📝 Stored Procedures: │ │     │  └──────────────────────────────┘ │
│  │    - sp_GetExpenses    │ │     │                                   │
│  │    - sp_CreateExpense  │ │     │  🔐 Role: Cognitive Services     │
│  │    - sp_UpdateExpense  │ │     │      OpenAI User                 │
│  │    - sp_DeleteExpense  │ │     └───────────────────────────────────┘
│  │    - sp_ApproveExpense │ │
│  │    - sp_FilterExpenses │ │
│  │    - (14 more...)      │ │
│  └────────────────────────┘ │
│                              │
│  🔐 Entra ID Only Auth       │
│  🔐 Managed Identity Access  │
│     - db_datareader          │
│     - db_datawriter          │
│     - EXECUTE permission     │
└──────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────────┐
│  AZURE MANAGED IDENTITY (User-Assigned)                                   │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  🆔 Identity: mid-expensemgmt-[unique]                             │  │
│  │                                                                     │  │
│  │  Permissions:                                                       │  │
│  │    ✓ App Service (assigned to)                                     │  │
│  │    ✓ Azure SQL Database (db_datareader, db_datawriter, EXECUTE)    │  │
│  │    ✓ Azure OpenAI (Cognitive Services OpenAI User role)            │  │
│  └────────────────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────────┐
│  DEPLOYMENT & MANAGEMENT                                                   │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  📦 Infrastructure as Code (Bicep)                                         │
│     - main.bicep                                                          │
│     - app-service.bicep                                                   │
│     - managed-identity.bicep                                              │
│     - azure-sql.bicep                                                     │
│     - genai.bicep                                                         │
│                                                                            │
│  🚀 Deployment Scripts                                                     │
│     - deploy.sh (app + db only)                                           │
│     - deploy-with-chat.sh (app + db + GenAI)                              │
│                                                                            │
│  🐍 Database Scripts (Python)                                              │
│     - run-sql.py (schema import)                                          │
│     - run-sql-dbrole.py (role assignment)                                 │
│     - run-sql-stored-procs.py (stored procedures)                         │
│                                                                            │
└───────────────────────────────────────────────────────────────────────────┘
```

## Data Flow

### 1. User Access Flow
```
User Browser ──> HTTPS ──> App Service ──> Razor Pages ──> UI Rendered
```

### 2. Database Query Flow
```
UI/API ──> DatabaseService ──> Stored Procedure ──> Azure SQL
                │                                      │
                │ Managed Identity Auth ───────────────┘
                │
                └──> Return Data ──> Format ──> Display
```

### 3. AI Chat Flow
```
User ──> Chat UI ──> ChatService ──> Azure OpenAI (GPT-4o)
                                          │
                Function Calling ─────────┤
                                          │
                                          ▼
                                    API Endpoints
                                          │
                                          ▼
                                   Stored Procedures
                                          │
                                          ▼
                                     Azure SQL DB
                                          │
                                          ▼
                                   Results to AI
                                          │
                                          ▼
                              Natural Language Response
                                          │
                                          ▼
                                    User receives answer
```

## Security Features

1. **Managed Identity Authentication**
   - No connection strings with passwords
   - Automatic credential rotation
   - Azure AD-based authentication

2. **Entra ID Only Authentication for SQL**
   - SQL authentication disabled
   - Azure AD users and managed identities only
   - Complies with MCAPS governance policies

3. **HTTPS Only**
   - All traffic encrypted in transit
   - TLS 1.2 minimum

4. **Role-Based Access**
   - Managed identity has minimal permissions
   - Database-level role assignments
   - Principle of least privilege

5. **Network Security**
   - SQL firewall rules for Azure services
   - App Service in secure network

## Key Technologies

| Component | Technology | Version |
|-----------|-----------|---------|
| Application Platform | ASP.NET Core Razor Pages | 8.0 (LTS) |
| Programming Language | C# | 12.0 |
| Database | Azure SQL Database | Basic Tier |
| AI Service | Azure OpenAI | GPT-4o |
| Hosting | Azure App Service (Linux) | S1 SKU |
| Identity | Azure Managed Identity | User-Assigned |
| IaC | Bicep | Latest |
| API Documentation | Swagger/OpenAPI | 3.0 |

## Deployment Regions

- **Primary Region**: UK South (uksouth)
  - App Service
  - Azure SQL Database
  - Managed Identity

- **AI Region**: Sweden Central (swedencentral)
  - Azure OpenAI (GPT-4o)
  - Reason: Better quota availability for demos

## Cost Estimation (Monthly)

| Service | SKU | Est. Cost (USD) |
|---------|-----|-----------------|
| App Service | S1 Standard | ~$70 |
| Azure SQL Database | Basic | ~$5 |
| Azure OpenAI | S0 (pay-per-use) | ~$20-50* |
| Managed Identity | Free | $0 |
| **Total** | | **~$95-125/month** |

*Based on moderate usage

## Scalability

- **App Service**: Can scale up to Premium SKUs
- **Azure SQL**: Can upgrade to Standard/Premium tiers
- **Azure OpenAI**: Auto-scales based on load
- **Architecture**: Stateless design allows horizontal scaling

## High Availability

- **App Service**: Built-in redundancy in S1 tier
- **Azure SQL**: Automatic backups, 99.99% SLA
- **Azure OpenAI**: Multi-region failover capability

## Monitoring & Observability

- Application Insights integration
- Azure Monitor metrics
- SQL Database query performance insights
- Diagnostic logs for all services
