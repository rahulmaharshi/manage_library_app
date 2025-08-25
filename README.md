# Manage Library App — ASP.NET Core 8 REST API for Libraries

[![Releases](https://img.shields.io/badge/Releases-v1.0-blue?logo=github)](https://github.com/rahulmaharshi/manage_library_app/releases)

https://github.com/rahulmaharshi/manage_library_app/releases

![Library shelf image](https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?auto=format&fit=crop&w=1400&q=80)

Table of contents
- About this project
- Badges
- Key features
- Tech stack
- Architecture overview
- Domain model
- Authentication and roles
- API endpoints
- Database and migrations
- Local setup and quick start
- Docker setup
- Running releases (download and execute)
- Configuration
- Sample requests
- Error handling and status codes
- Logging and observability
- Tests
- CI / CD suggestions
- Security notes
- Contribution guide
- File layout
- License
- Releases

About this project
This repository contains a backend API for a compact library management system. The API uses ASP.NET Core 8. It stores books, authors, and publishers. It handles user accounts, roles, and borrowing records. The app targets SQL Server for storage. It supports JWT bearer authentication and role-based controls for Admin, Librarian, and Member users. The API follows RESTful design. The code uses Entity Framework Core for ORM and migrations. The project ships with OpenAPI (Swagger) documentation.

Badges
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Swagger](https://img.shields.io/badge/Docs-Swagger-yellow?logo=swagger)](/swagger/index.html)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-007ACC?logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)
[![NuGet](https://img.shields.io/badge/Packages-NuGet-orange?logo=nuget)](https://www.nuget.org/)

Key features
- Manage books with metadata: title, ISBN, copies, genres, language.
- Manage authors and publisher records.
- Track borrow records and due dates.
- Role-based access: Admin, Librarian, Member.
- JWT bearer tokens for stateless auth.
- Secure password storage with ASP.NET Core Identity.
- Paging, filtering, and sorting on list endpoints.
- Audit fields on key entities: created by, updated by, timestamps.
- EF Core migrations and seed data.
- OpenAPI docs with Swagger UI.
- Health checks and metrics endpoints.

Tech stack
- ASP.NET Core 8 (Web API)
- C# 12
- Entity Framework Core 8
- SQL Server
- ASP.NET Core Identity
- JWT bearer tokens
- AutoMapper
- MediatR (for CQRS patterns; optional)
- Swagger / Swashbuckle
- xUnit for tests
- Docker for container builds

Architecture overview
The app follows a layered API structure. Each layer uses simple responsibilities. Layers include:
- API layer: controllers, request validation, authentication, and response shaping.
- Application layer: business rules and service orchestrations.
- Domain layer: entities, value objects, and domain logic.
- Infrastructure layer: EF Core context, repositories, identity, and external integrations.
- Shared: DTOs, common helpers, and mapping profiles.

The app uses dependency injection. It keeps controllers thin. Business logic lives in services or handlers. The database uses EF Core migrations and the DbContext maps domain entities to tables. The API exposes REST endpoints that return JSON.

Domain model
Primary entities:
- Book
  - Id (GUID)
  - Title
  - ISBN
  - Summary
  - Language
  - Genres (comma list or separate table)
  - PublisherId
  - Authors (many-to-many)
  - TotalCopies
  - AvailableCopies
  - PublishedDate
  - Audit fields
- Author
  - Id (GUID)
  - FirstName
  - LastName
  - Biography
  - DateOfBirth
  - Books (many-to-many)
  - Audit fields
- Publisher
  - Id (GUID)
  - Name
  - Country
  - Website
  - Audit fields
- BorrowRecord
  - Id (GUID)
  - BookId
  - MemberId
  - BorrowedAt
  - DueAt
  - ReturnedAt (nullable)
  - Status (Borrowed, Returned, Overdue)
  - Audit fields
- ApplicationUser (Identity)
  - Id (string GUID)
  - Email
  - DisplayName
  - Role (Admin, Librarian, Member)
  - Profile fields

Relationships
- Book to Author is many-to-many.
- Book to Publisher is many-to-one.
- BorrowRecord links book to a member user.

Authentication and roles
The app uses ASP.NET Core Identity with JWT tokens. The identity system stores users and roles in SQL Server. The API issues access tokens on login. Use the following roles:
- Admin: manage system settings, seed data, users.
- Librarian: manage books, authors, publishers, and borrow records.
- Member: search catalogs and create borrow requests.

Flow
1. A client posts credentials to /api/auth/login.
2. The server validates credentials via Identity.
3. The server returns a JWT access token.
4. The client sends the token in an Authorization header: Authorization: Bearer <token>.
5. The API enforces role checks via [Authorize(Roles = "Librarian,Admin")] or policy.

Token claims
- sub: user id
- email: user email
- role: user role
- exp: expiry timestamp

API endpoints
The API follows REST style. Endpoints use plural nouns. Use common HTTP verbs. Below are core endpoints with short descriptions and request/response summaries.

Authentication
- POST /api/auth/register
  - Registers a new user.
  - Body includes name, email, password, role (Member by default).
- POST /api/auth/login
  - Issues a JWT token.
  - Body: email, password.
  - Response: accessToken, expiresIn.

Users
- GET /api/users
  - Admin only: list users with paging.
- GET /api/users/{id}
  - Get a user profile.
- PUT /api/users/{id}
  - Update user profile or role (Admin only).
- DELETE /api/users/{id}
  - Admin only: remove user.

Books
- GET /api/books
  - List books with optional filters: title, author, genre, publisher, available.
  - Query params: page, pageSize, sort.
- GET /api/books/{id}
  - Retrieve a book by id.
- POST /api/books
  - Librarian or Admin: create a book.
  - Body includes title, ISBN, authors, publisher, totalCopies.
- PUT /api/books/{id}
  - Update book metadata and copies.
- DELETE /api/books/{id}
  - Librarian or Admin: delete a book.

Authors
- GET /api/authors
  - List authors with paging.
- GET /api/authors/{id}
  - Get author details and their books.
- POST /api/authors
  - Librarian or Admin: create author.
- PUT /api/authors/{id}
  - Update author.
- DELETE /api/authors/{id}
  - Delete author.

Publishers
- GET /api/publishers
  - List publishers.
- POST /api/publishers
  - Create publisher.
- PUT /api/publishers/{id}
  - Update publisher.
- DELETE /api/publishers/{id}
  - Delete publisher.

Borrowing
- POST /api/borrow
  - Member: request to borrow a book.
  - Body: bookId, expectedDueDays.
  - The system creates a BorrowRecord, reduces AvailableCopies.
- PUT /api/borrow/{id}/return
  - Librarian or Admin: mark as returned.
  - The system sets ReturnedAt and increases AvailableCopies.
- GET /api/borrow
  - List borrow records. Librarian sees all records. Member sees own records.
- GET /api/borrow/overdue
  - Librarian or Admin: list overdue items.

Search
- GET /api/search
  - Unified endpoint to search books, authors, and publishers.

Health and diagnostics
- GET /health
  - Returns readiness and liveness checks.
- GET /metrics
  - Optional metrics for Prometheus.

Database and migrations
The project uses EF Core migrations. The DbContext contains DbSet for each entity. Use migrations to modify schema.

Typical commands
- Add a migration:
  - dotnet ef migrations add AddBorrowStatus
- Apply migrations:
  - dotnet ef database update
- Seed data
  - The project runs seed logic on startup in non-production mode. Seed adds sample users and sample books.

Migration tips
- Keep migrations small and focused.
- Use GUID keys for portability.
- Use shadow properties for audit fields if you prefer.

Local setup and quick start
Prerequisites
- .NET 8 SDK
- SQL Server or localdb
- Git
- Optional: Docker

Clone the repo
- git clone https://github.com/rahulmaharshi/manage_library_app.git
- cd manage_library_app

Set database connection
- Copy appsettings.Development.json and set ConnectionStrings:DefaultConnection to your SQL Server instance.

Run migrations
- dotnet tool install --global dotnet-ef
- dotnet ef database update

Run the app
- dotnet run --project src/ManageLibrary.Api
- The API starts on the configured port, e.g. https://localhost:5001.

Open Swagger UI
- Visit https://localhost:5001/swagger to view API docs and test endpoints.

Docker setup
The project includes a Dockerfile. Build and run with Docker.

Build image
- docker build -t manage-library-app:latest .

Run container with SQL Server
- docker run -e "ConnectionStrings__DefaultConnection=<your-conn-string>" -p 5001:80 manage-library-app:latest

Use docker-compose
- The repo provides a docker-compose.yml that sets up SQL Server and the API. Use docker-compose up.

Running releases (download and execute)
You can download compiled releases from the project releases page. Download the release file and execute it.

- Visit and download the release file: https://github.com/rahulmaharshi/manage_library_app/releases
- After downloading the release archive or executable, extract it if needed.
- Run the executable or start the container inside the release package.
- On Linux: chmod +x manage-library-app && ./manage-library-app
- On Windows: run the .exe from PowerShell or CMD.

Use the release badge to open the releases page:
[![Download Release](https://img.shields.io/badge/Download-Releases-blue?logo=github)](https://github.com/rahulmaharshi/manage_library_app/releases)

Configuration
Use appsettings.json to configure runtime values. Common settings:
- ConnectionStrings: DefaultConnection
- Jwt: Key, Issuer, Audience, TokenLifetimeMinutes
- Logging: LogLevel
- AllowedHosts

Environment overrides
- Use appsettings.Development.json for dev settings.
- Use environment variables for secret values in production.
- For containerized deployment, pass env vars in docker run or compose.

Sample configuration snippet
- "Jwt": { "Key": "<very-secure-key>", "Issuer": "ManageLibrary", "Audience": "ManageLibraryClient", "TokenLifetimeMinutes": 60 }

Sample requests
Below are concise examples. Replace placeholders with real values.

Register a user
POST /api/auth/register
Body:
{
  "displayName": "Jane Doe",
  "email": "jane@example.com",
  "password": "P@ssw0rd",
  "role": "Member"
}

Login and obtain token
POST /api/auth/login
Body:
{
  "email": "jane@example.com",
  "password": "P@ssw0rd"
}
Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}

Use token
GET /api/books
Header:
Authorization: Bearer eyJ...

Create a book (Librarian role)
POST /api/books
Header:
Authorization: Bearer <librarian-token>
Body:
{
  "title": "Design Patterns",
  "isbn": "978-0201633610",
  "authors": ["Erich Gamma", "Richard Helm"],
  "publisherId": "b3f8b1f2-6d3a-4b1f-a9b6-1234567890ab",
  "totalCopies": 5,
  "publishedDate": "1994-10-31",
  "genres": ["Software", "Architecture"]
}

Borrow a book (Member)
POST /api/borrow
Header:
Authorization: Bearer <member-token>
Body:
{
  "bookId": "c12a6b2b-4b7f-44cc-9b8f-abcdef123456",
  "expectedDueDays": 14
}

Return a book (Librarian)
PUT /api/borrow/{id}/return
Header:
Authorization: Bearer <librarian-token>

Example curl
curl -X POST "https://localhost:5001/api/auth/login" -H "Content-Type: application/json" -d '{"email":"jane@example.com","password":"P@ssw0rd"}'

Error handling and status codes
The API returns HTTP status codes that follow REST patterns.

- 200 OK: Successful GET or POST that returns resources.
- 201 Created: Successful resource creation. Response includes Location header.
- 204 No Content: Successful update or delete without body.
- 400 Bad Request: Validation errors or malformed payload.
- 401 Unauthorized: Missing or invalid token.
- 403 Forbidden: Authenticated but lacks required role.
- 404 Not Found: Resource not found.
- 409 Conflict: Entity conflict, e.g., duplicate ISBN.
- 500 Internal Server Error: Unhandled server error.

Validation errors return a structured JSON object with a list of errors. Each error contains a field and a message. The API uses data annotations and FluentValidation for request validation.

Logging and observability
The app uses built-in ASP.NET Core logging. The default sink writes to console. You can add sinks for files, Seq, or ElasticSearch.

Health checks
- The /health endpoint returns simple checks for:
  - Database connectivity
  - Optional external services
  - Migration status

Metrics
- The project can expose Prometheus-style metrics using Prometheus-net or OpenTelemetry.

Tracing
- Add OpenTelemetry instrumentation to collect traces for HTTP calls, EF Core, and custom spans.

Tests
The repo includes unit and integration tests. Tests use xUnit and FluentAssertions.

Run tests
- dotnet test

Test types
- Unit tests: service logic and helpers.
- Integration tests: controllers with in-memory or test database.
- End-to-end tests: run against a containerized system.

CI / CD suggestions
- Use GitHub Actions to run tests on push and PR.
- Build matrix for dotnet SDK versions and OS.
- Use a build job to create a Docker image and push to a registry.
- Use a deploy job to push to Kubernetes or to a VM.
- Run security scans for dependencies.

Sample GitHub Actions workflow steps
- Checkout code.
- Setup .NET SDK.
- Restore and build.
- Run dotnet format and tests.
- Build Docker image and push on release tag.

Security notes
- Store Jwt:Key in a secure vault or environment variable.
- Use HTTPS in production.
- Do not log sensitive data like passwords.
- Rotate tokens if you detect a leak.
- Validate user input and sanitize outputs.
- Apply role checks on any write endpoint.

Performance tips
- Use paging on list endpoints.
- Add indexes on frequently queried columns like ISBN, AuthorId, and PublisherId.
- Cache read-heavy endpoints with a short TTL.
- Use async EF Core methods.

Data seeding and sample data
The project seeds:
- Admin user with a secure default password (please change).
- Librarian user.
- Several sample authors, publishers, and books.
- A few borrow records for demo.

File layout
A typical layout:
- src/
  - ManageLibrary.Api/         -> Web API project
  - ManageLibrary.Application/ -> Services and handlers
  - ManageLibrary.Domain/      -> Entities and domain logic
  - ManageLibrary.Infrastructure/ -> EF Core, Identity, email, logging
  - ManageLibrary.Tests/       -> Unit and integration tests
- docker-compose.yml
- README.md
- .github/workflows/ci.yml

Coding conventions
- Use async/await for I/O.
- Use CancellationToken on public async methods.
- Keep controllers lean.
- Use DTOs for requests and responses.
- Use AutoMapper or map manually for clarity.
- Use constructor injection for services.

Mapping and DTOs
Use DTOs to decouple internal entities from API contracts. Examples:
- BookDto: contains id, title, isbn, availableCopies.
- CreateBookRequest: contains title, isbn, authorIds, publisherId, totalCopies.

Migrations and schema evolution
- Prefer additive migrations.
- Script destructive migrations with care.
- Back up the database before applying destructive changes.

Seed script example (pseudo)
- If no users exist, create Admin and Librarian roles and users.
- Add sample publishers and authors.
- Add sample books and set copies.

Common commands recap
- Restore: dotnet restore
- Build: dotnet build
- Run: dotnet run --project src/ManageLibrary.Api
- Test: dotnet test
- Add migration: dotnet ef migrations add MigrationName --project src/ManageLibrary.Infrastructure
- Update database: dotnet ef database update --project src/ManageLibrary.Infrastructure

Swagger UI and API docs
Swagger runs at /swagger by default. Use the UI to:
- Explore endpoints.
- Try sample requests.
- View models and schemas.

Postman collection
The repo includes a Postman collection in /docs/postman. Import it to test flows and endpoints faster.

Sample policies
- Use policies for more complex authorization. Example:
  - Policy for BorrowManagement requires role Librarian or Admin and a claim "CanManageBorrow".

Data retention
- Keep borrow records for an audit period if needed.
- Consider a soft delete strategy for books and authors to preserve history.

Internationalization
- Store language and culture codes on books.
- Use UTC for date storage. Convert to local time on the client.

Rate limiting
- Add a rate limiter for public or unauthenticated endpoints.
- Use sliding windows or token buckets.

Extending the project
- Add reservations for future borrow.
- Add fines and payment integration.
- Add import/export for CSV catalogs.
- Add notifications for due soon and overdue.
- Integrate a search index (ElasticSearch) for full text queries.

Contribution guide
- Fork the repo.
- Create a feature branch: feature/your-feature.
- Follow the coding conventions.
- Add tests for new behavior.
- Create a PR to main. Describe your change and link issue.

Issue reporting
- Open an issue with:
  - Short description.
  - Steps to reproduce.
  - Expected and actual behavior.
  - Logs or stack traces when possible.

Branching model
- Use main as the stable branch.
- Use feature branches for work.
- Use release tags for published builds.

Release process
- Tag a release in GitHub.
- Build artifacts and attach them to the GitHub release.
- Update release notes with changes and migration steps.

Releases
You can download compiled releases from the releases page. Download the file and execute it.

Download and run the release file:
- Visit https://github.com/rahulmaharshi/manage_library_app/releases
- Download the latest archive or executable.
- Extract and run the executable or use the provided Docker image.

Direct link to releases:
[https://github.com/rahulmaharshi/manage_library_app/releases](https://github.com/rahulmaharshi/manage_library_app/releases)

Contact and support
- Open an issue on GitHub for bugs or questions.
- Use PRs for fixes or improvements.

License
This repository includes a LICENSE file. Check the file for full terms.

Images and emojis
- Use book, library, and code emojis to highlight sections:
  - 📚 Books
  - 🧾 Authors
  - 🏛️ Publisher
  - 🔐 Auth
  - 🧩 API
  - ⚙️ Configuration
  - 🐳 Docker

Appendix: example JSON payloads
CreateBook
{
  "title": "Clean Architecture",
  "isbn": "978-0134494166",
  "summary": "A guide to software architecture",
  "language": "en",
  "genres": ["Software", "Architecture"],
  "authorIds": ["a1b2c3d4-1111-2222-3333-444455556666"],
  "publisherId": "p1q2w3e4-1111-2222-3333-444455556666",
  "totalCopies": 4,
  "publishedDate": "2017-09-20"
}

CreateAuthor
{
  "firstName": "Robert",
  "lastName": "Martin",
  "biography": "Author and software craftsman",
  "dateOfBirth": "1952-12-05"
}

BorrowRequest
{
  "bookId": "b3f8b1f2-6d3a-4b1f-a9b6-1234567890ab",
  "expectedDueDays": 21
}

Common migration scenario
- Add new column to Book for edition.
- Create migration with EF Core.
- Update database.
- Seed edition data for existing records via migration script.

Checklist before production deploy
- Secure Jwt key in vault.
- Enable HTTPS and HSTS.
- Set proper CORS policy.
- Configure logging to a durable sink.
- Run database backups and migration plan.
- Validate seed data and admin accounts.

Useful links
- Official .NET: https://dotnet.microsoft.com/
- ASP.NET Core Identity docs: https://learn.microsoft.com/aspnet/core/security/authentication/identity
- EF Core docs: https://learn.microsoft.com/ef/core/
- OpenAPI / Swagger: https://swagger.io/

Releases and downloads
Download release artifacts from the GitHub releases page. After you download, run the file included in the release package.

- Releases: https://github.com/rahulmaharshi/manage_library_app/releases

