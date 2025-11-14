# Customer Order Insight Tracker

Customer Order Insight Tracker is a lightweight web application built with ASP.NET Core, designed to provide quick insights into the Northwind database. It allows users to search for customers by country and view their corresponding order history. The application also features a custom middleware to track and log all incoming web requests.

## **Architecture Overview**

The project follows a traditional server-rendered web application model, enhanced with client-side libraries for a dynamic user experience.

*   **Backend**: A robust API and web server built with **C#** and **ASP.NET Core 8**. It uses an **Entity Framework Core** data layer to communicate with a **SQL Server** database. A **CoreWCF** service is also implemented to expose business logic, although the frontend primarily communicates via MVC controller actions.
*   **Frontend**: A server-rendered UI using **Razor Views (.cshtml)**. The user interface is made interactive with **AngularJS** for handling user input and **Kendo UI** for displaying data in powerful grids. The layout is styled using **Bootstrap**.
*   **Database**: The application relies on the classic **Northwind** database, which is set up manually on a local SQL Server instance.
*   **Resilience & Fallback Strategy**: The application automatically checks the SQL Server connection at startup (with 3 retry attempts). If the connection fails, it **automatically falls back to CSV files** (`data/customers.csv`, `data/orders.csv`) to ensure the application remains functional even without a database.
*   **Request Logging**: A custom **Action Filter** (`WebTrackerActionFilter`) intercepts all incoming requests and logs them to a CSV file (`data/webtracker.csv`) for auditing purposes.

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
cd /src/Creativa.Web

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
*   **CoreWCF**: Included to fulfill the requirement of exposing a WCF service. The service contract is defined in `INorthwindService.cs` and implemented in `NorthwindSqlService.cs`. While the frontend uses MVC actions, this demonstrates the ability to integrate legacy-style service contracts.
*   **MVC Action Filter for Logging**: A custom filter, `WebTrackerActionFilter`, was implemented to meet the request tracking requirement. This is a clean, non-invasive approach that automatically intercepts all controller actions without modifying their code.

### **Frontend**

*   **Razor Views**: The standard server-side templating engine for ASP.NET Core. It allows for generating HTML on the server, which is ideal for this project's scope.
*   **AngularJS & Kendo UI**: Chosen to meet the specific requirements of the technical challenge. AngularJS handles the client-side logic for the search functionality, while Kendo UI provides powerful and feature-rich data grids (`kendoGrid`) for displaying customer and order data.
*   **jQuery**: Used as a dependency for both Bootstrap and Kendo UI, and for simplifying DOM manipulation in the Razor views.
*   **JSON Serialization (`camelCase`)**: The backend is explicitly configured in `Program.cs` to serialize JSON responses with `camelCase` property names. This is a modern web standard and ensures seamless compatibility with JavaScript libraries like Kendo UI, which expect this format.

## **API Testing Guide (cURL)**

You can test the backend endpoints directly using a command-line tool like `cURL`.

### **1. Get Customers by Country**

This endpoint retrieves a list of customers for a given country.

*   **Request**:
    ```bash
    curl -s "http://localhost:5084/Customers/CustomersByCountryData?country=Germany"
    ```

*   **Expected Response (200 OK)**: A JSON array of customer objects.
    ```json
    [
      {
        "customerID": "ALFKI",
        "companyName": "Alfreds Futterkiste",
        "contactName": "Maria Anders",
        "country": "Germany",
        "phone": "030-0074321",
        "fax": "030-0076545"
      },
      {
        "customerID": "BLAUS",
        "companyName": "Blauer See Delikatessen",
        "contactName": "Hanna Moos",
        "country": "Germany",
        "phone": "0621-08460",
        "fax": "0621-08924"
      }
    ]
    ```

### **2. Get Orders for a Customer**

This endpoint retrieves the HTML page containing the orders for a specific customer.

*   **Request**:
    ```bash
    curl "http://localhost:5084/Customers/CustomerOrdersInformation?id=ALFKI"
    ```

*   **Expected Response (200 OK)**: The full HTML content of the customer orders page. The response will contain a Kendo Grid populated with the order data for customer `ALFKI`.

### **3. Check Request Tracking**

After making a few requests, you can verify that they were logged.

*   **Request**:
    ```bash
    tail -n 5 data/webtracker.csv
    ```

*   **Expected Response**: The last few lines of the CSV log file, showing the tracked requests.
    ```csv
    43,/Customers/CustomersByCountry?country=Germany,::1,2025-11-14T05:24:40.6500319Z
    44,/Customers/CustomerOrdersInformation?id=ALFKI,::1,2025-11-14T05:24:49.5013795Z
    ```

You can visualize the logs at `data/webtracker.csv`

## **Key Project Files**

| File Path                                                                       | Description                                                                                             |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `src/Creativa.Web/Program.cs`                                                   | Main application entry point. Configures services, dependency injection, and middleware pipeline.     |
| `src/Creativa.Web/Controllers/CustomersController.cs`                           | MVC controller that handles HTTP requests for customers and orders.                                     |
| `src/Creativa.Web/Data/NorthwindContext.cs`                                     | Entity Framework Core `DbContext` for interacting with the Northwind database.                        |
| `src/Creativa.Web/Services/NorthwindSqlService.cs`                              | Service class containing the business logic to query customer and order data via EF Core.             |
| `src/Creativa.Web/Views/Customers/CustomersByCountry.cshtml`                    | Razor view with the AngularJS and Kendo UI grid for searching and displaying customers.               |
| `src/Creativa.Web/Views/Customers/CustomerOrdersInformation.cshtml`             | Razor view that displays the Kendo UI grid for a specific customer's orders.                          |
| `src/Creativa.Web/Filters/WebTrackerActionFilter.cs`                            | The action filter responsible for intercepting and logging all requests.                              |
| `instnwnd.sql`                                                                  | The SQL script used to set up and populate the Northwind database.                                    |
| `data/webtracker.csv`                                                           | The CSV file where all tracked web requests are stored.                                                 |
