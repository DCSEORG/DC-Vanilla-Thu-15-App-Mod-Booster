#!/bin/bash

# Deploy Expense Management Application (without GenAI)
# This script deploys the infrastructure, database, and application

set -e  # Exit on any error

echo "======================================"
echo "Expense Management Deployment"
echo "======================================"
echo ""

# Configuration
RESOURCE_GROUP="rg-expensemgmt-demo"
LOCATION="uksouth"
DEPLOYMENT_NAME="expensemgmt-deployment-$(date +%s)"

# Get current user info for SQL admin
echo "Getting current user information..."
CURRENT_USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
CURRENT_USER_LOGIN=$(az ad signed-in-user show --query userPrincipalName -o tsv)

echo "Current User Object ID: $CURRENT_USER_OBJECT_ID"
echo "Current User Login: $CURRENT_USER_LOGIN"
echo ""

# Create resource group
echo "Creating resource group: $RESOURCE_GROUP in $LOCATION..."
az group create --name $RESOURCE_GROUP --location $LOCATION --output none
echo "✓ Resource group created"
echo ""

# Deploy infrastructure (without GenAI)
echo "Deploying infrastructure (App Service, Managed Identity, Azure SQL)..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group $RESOURCE_GROUP \
    --name $DEPLOYMENT_NAME \
    --template-file infrastructure/main.bicep \
    --parameters location=$LOCATION \
    --parameters adminObjectId=$CURRENT_USER_OBJECT_ID \
    --parameters adminLogin=$CURRENT_USER_LOGIN \
    --parameters deployGenAI=false \
    --query 'properties.outputs' \
    --output json)

echo "✓ Infrastructure deployed"
echo ""

# Extract outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
APP_SERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceUrl.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
DATABASE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.databaseName.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')

echo "Deployment Outputs:"
echo "  App Service Name: $APP_SERVICE_NAME"
echo "  App Service URL: $APP_SERVICE_URL"
echo "  SQL Server FQDN: $SQL_SERVER_FQDN"
echo "  Database Name: $DATABASE_NAME"
echo "  Managed Identity Client ID: $MANAGED_IDENTITY_CLIENT_ID"
echo ""

# Wait for SQL Server to be ready
echo "Waiting 30 seconds for SQL Server to be fully ready..."
sleep 30
echo ""

# Add current IP to SQL firewall
echo "Adding current IP to SQL firewall..."
MY_IP=$(curl -s https://api.ipify.org)
SQL_SERVER_NAME=$(echo $SQL_SERVER_FQDN | cut -d'.' -f1)

# Allow Azure services access
echo "Allowing Azure services access to SQL Server..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name "AllowAllAzureIPs" \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0 \
    --output none

# Add deployment IP
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name "AllowDeploymentIP" \
    --start-ip-address $MY_IP \
    --end-ip-address $MY_IP \
    --output none

echo "✓ Firewall rules configured"
echo ""

echo "Waiting additional 15 seconds for firewall rules to propagate..."
sleep 15
echo ""

# Update Python scripts with actual server and database names
echo "Updating Python scripts with deployment values..."
sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/$SQL_SERVER_FQDN/g" run-sql.py && rm -f run-sql.py.bak
sed -i.bak "s/expensedb/$DATABASE_NAME/g" run-sql.py && rm -f run-sql.py.bak

sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/$SQL_SERVER_FQDN/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak
sed -i.bak "s/expensedb/$DATABASE_NAME/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak

sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/$SQL_SERVER_FQDN/g" run-sql-stored-procs.py && rm -f run-sql-stored-procs.py.bak
sed -i.bak "s/expensedb/$DATABASE_NAME/g" run-sql-stored-procs.py && rm -f run-sql-stored-procs.py.bak
echo "✓ Python scripts updated"
echo ""

# Update script.sql with managed identity name
MANAGED_IDENTITY_NAME="mid-expensemgmt-$(echo $MANAGED_IDENTITY_CLIENT_ID | cut -d'-' -f1)"
sed -i.bak "s/MANAGED-IDENTITY-NAME/$MANAGED_IDENTITY_NAME/g" script.sql && rm -f script.sql.bak
echo "✓ SQL script updated with managed identity name"
echo ""

# Install Python dependencies
echo "Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity
echo "✓ Python dependencies installed"
echo ""

# Import database schema
echo "Importing database schema..."
python3 run-sql.py
echo "✓ Database schema imported"
echo ""

# Configure database roles for managed identity
echo "Configuring database roles for managed identity..."
python3 run-sql-dbrole.py
echo "✓ Database roles configured"
echo ""

# Deploy stored procedures
echo "Deploying stored procedures..."
python3 run-sql-stored-procs.py
echo "✓ Stored procedures deployed"
echo ""

# Build and publish the application
echo "Building and publishing application..."
cd ExpenseManagementApp/ExpenseManagement
dotnet publish -c Release -o ../../publish
cd ../..
echo "✓ Application built"
echo ""

# Create deployment package
echo "Creating deployment package..."
cd publish
zip -r ../app.zip . > /dev/null
cd ..
echo "✓ Deployment package created"
echo ""

# Configure App Service settings
echo "Configuring App Service settings..."
CONNECTION_STRING="Server=tcp:$SQL_SERVER_FQDN,1433;Database=$DATABASE_NAME;Authentication=Active Directory Managed Identity;User Id=$MANAGED_IDENTITY_CLIENT_ID;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --settings \
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
        "ManagedIdentityClientId=$MANAGED_IDENTITY_CLIENT_ID" \
        "AZURE_CLIENT_ID=$MANAGED_IDENTITY_CLIENT_ID" \
    --output none

echo "✓ App Service settings configured"
echo ""

# Deploy application
echo "Deploying application to App Service..."
az webapp deploy \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --src-path ./app.zip \
    --type zip \
    --async true

echo "✓ Application deployment initiated"
echo ""

echo "======================================"
echo "Deployment Complete!"
echo "======================================"
echo ""
echo "📋 Summary:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  App Service: $APP_SERVICE_NAME"
echo "  Database: $SQL_SERVER_FQDN/$DATABASE_NAME"
echo "  Managed Identity: $MANAGED_IDENTITY_CLIENT_ID"
echo ""
echo "🌐 Access your application:"
echo "  Main App: $APP_SERVICE_URL/Index"
echo "  Swagger API: $APP_SERVICE_URL/swagger"
echo "  Chat UI: $APP_SERVICE_URL/Chat"
echo ""
echo "⚠️  Note: The Chat UI will show a message that GenAI services were not deployed."
echo "    To enable full AI chat features, run: ./deploy-with-chat.sh"
echo ""
echo "🔄 Application deployment is completing in the background."
echo "   It may take 2-3 minutes for the app to be fully available."
echo ""
