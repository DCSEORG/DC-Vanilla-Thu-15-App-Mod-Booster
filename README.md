![Header image](https://github.com/DougChisholm/App-Mod-Booster/blob/main/repo-header-booster.png)

# Expense Management System - Modern Azure Cloud Solution

A modern, cloud-native expense management application built with ASP.NET Core 8, Azure SQL Database, and Azure OpenAI for natural language interactions.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-Cloud-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)

## 🌟 Features

### Core Functionality
- ✅ **Add Expenses**: Submit expenses with amount, date, category, and description
- 📋 **View Expenses**: List all expenses with filtering capabilities
- ✏️ **Edit Expenses**: Update expense details
- 🗑️ **Delete Expenses**: Remove expenses from the system
- ✅ **Approve/Reject**: Managers can review and approve/reject submitted expenses
- 🔍 **Filter & Search**: Filter expenses by status, category, date range

### Technical Features
- 🔐 **Managed Identity Authentication**: Secure, password-less authentication
- 🗄️ **Stored Procedures**: All database operations via stored procedures
- 📡 **REST APIs**: Complete CRUD APIs with Swagger documentation
- 🤖 **AI Chat Assistant**: Natural language queries powered by Azure OpenAI GPT-4o
- 🎨 **Modern UI**: Clean, responsive design with status badges and animations
- ⚡ **Error Handling**: Detailed error messages with dummy data fallback
- 🏗️ **IaC**: Infrastructure as Code using Azure Bicep

## 🏗️ Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture diagrams and explanations.

### High-Level Overview

```
Users ──> App Service ──> Azure SQL Database
              │
              └──> Azure OpenAI (GPT-4o)
```

- **Frontend**: ASP.NET Core 8 Razor Pages
- **Backend**: REST APIs with Swagger
- **Database**: Azure SQL Database (Entra ID auth only)
- **AI**: Azure OpenAI GPT-4o with function calling
- **Security**: User-Assigned Managed Identity

## 📋 Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (v2.40+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Python 3](https://www.python.org/downloads/) (for database scripts)
- Azure subscription with permissions to create resources
- `jq` command-line tool for JSON parsing

## 🚀 Quick Start

### Option 1: Deploy Without AI Chat (Faster - ~5 minutes)

```bash
# Login to Azure
az login

# Deploy
./deploy.sh
```

### Option 2: Deploy With AI Chat (Full Features - ~8 minutes)

```bash
# Login to Azure
az login

# Deploy with GenAI
./deploy-with-chat.sh
```

## 📂 Project Structure

```
.
├── infrastructure/              # Bicep templates
│   ├── main.bicep              # Main orchestration
│   ├── app-service.bicep       # App Service configuration
│   ├── managed-identity.bicep  # Managed Identity
│   ├── azure-sql.bicep         # SQL Database
│   └── genai.bicep             # Azure OpenAI
├── ExpenseManagementApp/       # .NET Application
│   └── ExpenseManagement/
│       ├── Models/             # Data models
│       ├── Services/           # Business logic
│       ├── Pages/              # Razor Pages UI
│       ├── Controllers/        # API Controllers
│       └── wwwroot/            # Static files
├── Database-Schema/            # Database schema
│   └── database_schema.sql     # Tables and seed data
├── stored-procedures.sql       # All stored procedures
├── deploy.sh                   # Deployment without GenAI
├── deploy-with-chat.sh        # Deployment with GenAI
├── ARCHITECTURE.md             # Architecture documentation
└── README.md                   # This file
```

## 🎯 Usage

### Accessing the Application

After deployment, you'll see URLs for:

1. **Main Application**: `https://<app-name>.azurewebsites.net/Index`
   - View and manage expenses
   - Add new expenses  
   - Edit existing expenses
   - Approve/reject pending expenses (managers)

2. **Swagger API**: `https://<app-name>.azurewebsites.net/swagger`
   - Interactive API documentation
   - Test API endpoints directly
   - View request/response schemas

3. **AI Chat**: `https://<app-name>.azurewebsites.net/Chat`
   - Natural language queries
   - Ask questions like:
     - "Show me all pending expenses"
     - "What's the total of approved expenses this month?"
     - "List travel expenses over £100"

## 🗄️ Database Schema

The application uses the following tables:

- **Users**: User information with roles
- **Roles**: Employee and Manager roles
- **Expenses**: Main expense records (amounts stored in pence)
- **ExpenseCategories**: Travel, Meals, Supplies, Accommodation, Other
- **ExpenseStatus**: Draft, Submitted, Approved, Rejected

All data access is performed through 18 stored procedures for security and performance.

## 🔐 Security

### Managed Identity Authentication

The application uses Azure Managed Identity for all authentication - no passwords or connection strings with credentials!

### Entra ID Only Authentication

Azure SQL Database is configured with Entra ID-only authentication:
- ✅ SQL authentication disabled
- ✅ Azure AD users and managed identities only
- ✅ Complies with MCAPS governance policies

## 💰 Cost Management

### Estimated Monthly Costs

- **App Service (S1)**: ~$70
- **Azure SQL (Basic)**: ~$5
- **Azure OpenAI**: ~$20-50 (usage-based)
- **Total**: ~$95-125/month

## 📚 Additional Resources

- [Architecture Documentation](ARCHITECTURE.md)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure OpenAI Service Documentation](https://docs.microsoft.com/en-us/azure/ai-services/openai/)

---

## 📖 About App-Mod-Booster

This project demonstrates how GitHub Copilot agent can turn screenshots of a legacy app into a working proof-of-concept for a cloud native Azure replacement when provided with the legacy database schema.

### Steps to modernise your own app:

1. Fork this repo 
2. In new repo replace the screenshots and sql schema
3. Open the coding agent and use app-mod-booster agent telling it "modernise my app"
4. When the app code is generated (can take up to 30 minutes) there will be a pull request to approve
5. Use codespaces to deploy the app to Azure (or clone locally with devcontainer)
6. Open terminal and type `az login` to set subscription/context
7. Run `./deploy.sh` or `./deploy-with-chat.sh` to deploy

Supporting slides for Microsoft Employees: [Here](<https://microsofteur-my.sharepoint.com/:p:/g/personal/dchisholm_microsoft_com/IQAY41LQ12fjSIfFz3ha4hfFAZc7JQQuWaOrF7ObgxRK6f4?e=p6arJs>)

---

**Built with ❤️ using Azure, .NET 8, and modern cloud practices**
