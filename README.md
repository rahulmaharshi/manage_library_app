# Library Management System API

This is a backend API for a simple library management system, built using ASP.NET Core 8. It provides core functionalities for managing books, authors, and publishers, as well as handling user authentication (Admin, Librarian, Member) and borrowing records.

## Features

- **User Authentication**: Secure API endpoints with JWT (JSON Web Tokens) for role-based access control (Admin, Librarian, Member).
- **Book Management**: Create, read, update, and delete (CRUD) books, authors, and publishers.
- **Bulk Data Insertion**: A dedicated endpoint to import a large number of books from a CSV file.
- **Borrowing System**: Members can request to borrow books, and Librarians can approve/return them.
- **Pagination & Searching**: APIs for listing books and borrowing records support pagination and search functionalities.

## Technologies

- **Backend**: ASP.NET Core 8
- **Database**: SQL Server (via Entity Framework Core)
- **Authentication**: JWT Bearer Tokens, ASP.NET Core Identity
- **Data Handling**: CSVHelper for parsing CSV files
- **API Documentation**: Swagger/OpenAPI

## Getting Started

To run this project locally, you will need:
1. .NET 8 SDK
2. SQL Server (or another database supported by Entity Framework Core)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/phamvangjang/manage_library_app
   cd manage_library_app

2. Update the database connection string in appsettings.json

3. Apply the database migrations:
    ```bash
    dotnet ef database update

4. Run the application:
    ```bash
    dotnet run

The API documentation will be available at https://localhost:7110/swagger (or the port configured in your project).