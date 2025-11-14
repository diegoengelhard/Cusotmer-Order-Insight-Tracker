# Customer Order Insight Tracker

Customer Order Insight Tracker is a lightweight web application built with ASP.NET Core, designed to provide quick insights into the Northwind database. It allows users to search for customers by country and view their corresponding order history. The application also features a custom action filter to track and log all incoming web requests directly to a SQL Server database.

## **Architecture Overview**

The project follows a traditional server-rendered web application model, enhanced with client-side libraries for a dynamic user experience.

*   **Backend**: A robust API and web server built with **C#** and **ASP.NET Core 8**. It uses an **Entity Framework Core** data layer to communicate with a **SQL Server** database. A **CoreWCF** service is implemented to expose all business logic via SOAP.
*   **Frontend**: A server-rendered UI using **Razor Views (.cshtml)**. The user interface is made interactive with **AngularJS** for handling user input and **Kendo UI** for displaying data in powerful grids. The Kendo DataSource is configured to communicate **directly with the CoreWCF SOAP service**.
*   **Database**: The application relies on the classic **Northwind** database, which is set up manually on a local SQL Server instance.
*   **Resilience & Fallback Strategy**: The application automatically checks the SQL Server connection at startup (with 3 retry attempts). If the connection fails, it **automatically falls back to local CSV files** (`data/customers.csv`, `data/orders.csv`) to ensure the application remains functional even without a database.
*   **Request Logging**: A custom **Action Filter** (`WebTrackerActionFilter`) intercepts all incoming requests and logs them to the `webTracker` table in the **SQL Server database** for auditing purposes.

## **Local Setup and Execution Guide**

Follow these steps to get the project running on your local machine.

### **Prerequisites**

*   .NET 8 SDK
*   SQL Server (a local instance, or Docker container) - **Optional** (see fallback behavior below)
*   `sqlcmd` utility (part of SQL Server command-line tools) - **Optional**

### **1. Database Setup (Optional)**

The application requires the Northwind database **only if you want to use SQL Server**. If SQL Server is unavailable, the app will automatically use the CSV files in the `data/` directory.

```bash
# 1. Navigate to the project root directory
cd /path/to/creativa-tech-int

# 2. Create the Northwind database
# Replace 'YourStrong@Passw0rd123' with your actual SA password
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 \
  -U sa \
  -P 'YourStrong@Passw0rd123' \
  -C \
  -Q "CREATE DATABASE Northwind"

# 3. Populate the database using the provided script
/opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 \
  -U sa \
  -P 'YourStrong@Passw0rd123' \
  -C \
  -d Northwind \
  -i instnwnd.sql
```

### **2. Backend & Frontend Execution**

The ASP.NET Core project serves both the backend logic and the frontend views.

```bash
# 1. Navigate to the web project directory
cd src/Creativa.Web

# 2. Restore dependencies and build the project
dotnet build

# 3. Run the application
dotnet run
```

Once the server is running, you can access the application by navigating to **`http://localhost:5084`** in your web browser.

## **💡 Technical Decisions**

This section outlines the key technical choices made during the project's development.

### **Backend**

*   **ASP.NET Core 8**: Chosen for its high performance, cross-platform capabilities, and modern development features like built-in dependency injection and configuration management.
*   **Entity Framework Core**: Used as the Object-Relational Mapper (ORM) to abstract database interactions. This allows for writing clean, strongly-typed data access code in C# instead of raw SQL. The database context and models are defined in `src/Creativa.Web/Data/NorthwindContext.cs`.
*   **SQL Server**: A robust, enterprise-grade relational database that is a natural fit for the .NET ecosystem and the provided `instnwnd.sql` script.
*   **CoreWCF**: Included to fulfill the requirement of exposing a WCF service. The service contract is defined in `INorthwindService.cs` and is directly consumed by the frontend's Kendo DataSource, demonstrating the ability to integrate legacy-style service contracts in a modern web application.
*   **Action Filter for Logging**: A custom filter, `WebTrackerActionFilter`, was implemented to meet the request tracking requirement. This is a clean, non-invasive approach that automatically intercepts all controller actions and logs the request details to the `webTracker` table in SQL Server.

### **Frontend**

*   **Razor Views**: The standard server-side templating engine for ASP.NET Core. It allows for generating HTML on the server, which is ideal for this project's scope.
*   **AngularJS & Kendo UI**: Chosen to meet the specific requirements of the technical challenge. AngularJS handles the client-side logic for the search functionality, while Kendo UI provides powerful and feature-rich data grids (`kendoGrid`) for displaying customer and order data.
*   **jQuery & Custom SOAP Helper**: jQuery is used as a dependency for Kendo UI. A custom helper, `wcf-helper.js`, was created to build SOAP envelopes, make AJAX calls to the WCF service, and parse the XML responses, correctly handling XML namespaces. This helper acts as the bridge between Kendo UI and the SOAP backend.

## **API Testing Guide (cURL)**

You can test the WCF service and check the database logging.

### **1. Get Customers by Country (SOAP)**

This endpoint retrieves a list of customers for a given country via a SOAP request.

*   **Request**:
    ```bash
    curl -X POST http://localhost:5084/NorthwindService/basichttp \
    -H "Content-Type: text/xml; charset=utf-8" \
    -H "SOAPAction: http://tempuri.org/INorthwindService/GetCustomersByCountry" \
    -d '<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/"><soap:Body><tem:GetCustomersByCountry><tem:country>Germany</tem:country></tem:GetCustomersByCountry></soap:Body></soap:Envelope>'
    ```

*   **Expected Response (200 OK)**: A SOAP envelope containing an array of customer objects.

### **2. Get Orders for a Customer (SOAP)**

This endpoint retrieves the orders for a specific customer via a SOAP request.

*   **Request**:
    ```bash
    curl -X POST http://localhost:5084/NorthwindService/basichttp \
    -H "Content-Type: text/xml; charset=utf-8" \
    -H "SOAPAction: http://tempuri.org/INorthwindService/GetOrdersByCustomer" \
    -d '<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/"><soap:Body><tem:GetOrdersByCustomer><tem:customerId>ALFKI</tem:customerId></tem:GetOrdersByCustomer></soap:Body></soap:Envelope>'
    ```

*   **Expected Response (200 OK)**: A SOAP envelope containing the order data for customer `ALFKI`.

### **3. Check Request Tracking in SQL Server**

After making a few requests in the web application, you can verify that they were logged in the database.

*   **Request (using `sqlcmd`)**:
    ```bash
    # Replace 'YourStrong@Passw0rd123' with your actual SA password
    /opt/mssql-tools18/bin/sqlcmd \
      -S localhost,1433 \
      -U sa \
      -P 'YourStrong@Passw0rd123' \
      -d Northwind \
      -Q "SELECT TOP 5 * FROM webTracker ORDER BY TimeOfAction DESC;"
    ```

*   **Expected Response**: The last few rows from the `webTracker` table, showing the tracked requests.

## **Key Project Files**

| File Path                                                                       | Description                                                                                             |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `src/Creativa.Web/Program.cs`                                                   | Main application entry point. Configures services, dependency injection, and the WCF middleware.      |
| `src/Creativa.Web/Services/INorthwindService.cs`                                | The WCF service contract defining the operations for getting customers and orders.                    |
| `src/Creativa.Web/Services/NorthwindSqlService.cs`                              | Service class implementing the WCF contract with business logic to query data via EF Core.            |
| `src/Creativa.Web/wwwroot/js/wcf-helper.js`                                     | Custom JavaScript helper to handle SOAP requests and parse XML responses for Kendo UI.                |
| `src/Creativa.Web/Views/Customers/CustomersByCountry.cshtml`                    | Razor view with the AngularJS and Kendo UI grid for searching and displaying customers via SOAP.      |
| `src/Creativa.Web/Views/Customers/CustomerOrdersInformation.cshtml`             | Razor view that displays the Kendo UI grid for a specific customer's orders, loaded via SOAP.         |
| `src/Creativa.Web/Filters/WebTrackerActionFilter.cs`                            | The action filter responsible for intercepting requests and logging them to the SQL database.         |
| `instnwnd.sql`                                                                  | The SQL script used to set up and populate the Northwind database, including the `webTracker` table.  |