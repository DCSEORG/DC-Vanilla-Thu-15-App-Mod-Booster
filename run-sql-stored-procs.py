#!/usr/bin/env python3
"""
Execute SQL script on Azure SQL Database using Azure Active Directory authentication
"""
from db_utils import execute_sql_script

# Database connection settings
SERVER = "sql-expensemgmt-placeholder.database.windows.net"
DATABASE = "expensedb"
SQL_SCRIPT_FILE = "stored-procedures.sql"

if __name__ == "__main__":
    try:
        execute_sql_script(SERVER, DATABASE, SQL_SCRIPT_FILE)
    except Exception as e:
        print(f"\n✗ Error: {e}")
        exit(1)
