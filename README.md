# Real Estate Site - Software Engineering Project 2025

This repository contains a Real Estate web application developed as part of the Software Engineering course (Fall 2025).

## Project Team

| Name                 | Student ID |
| -------------------- | ---------- |
| Alperen CETIN        | 210316026  |
| Fatma Sevval AYDOGAN | 210316057  |
| Irem BOYALIOGLU      | 200316003  |
| Elif Nida SOLAKOGLU  | 220316037  |

## Project Overview

A full-stack real estate listing management system built with ASP.NET Core 8.0. The application allows users to browse, create, and manage property listings with features like filtering, image uploads, and user authentication.

## Technology Stack

| Layer             | Technology                        |
| ----------------- | --------------------------------- |
| Backend API       | ASP.NET Core 8.0 Web API          |
| Frontend          | ASP.NET Core MVC, Razor Pages     |
| Database          | SQLite with Entity Framework Core |
| Authentication    | JWT Bearer Token                  |
| UI Framework      | Bootstrap 5                       |
| API Documentation | Swagger/OpenAPI                   |

## Features

- RESTful API with full CRUD operations
- JWT-based authentication and authorization
- Role-based access control (Admin/User)
- Property listing management (create, read, update, delete)
- Image upload support
- Filtering by property type (Sale/Rent)
- Responsive web design
- Swagger API documentation

## Project Structure

```
RealEstate/
├── RealEstate.Api/              # Backend Web API
│   ├── Controllers/             # API endpoints
│   ├── Data/                    # Database context
│   ├── Dtos/                    # Data transfer objects
│   ├── Entities/                # Database entities
│   ├── Services/                # Business logic
│   └── Migrations/              # EF Core migrations
│
├── RealEstate.Web/              # Frontend MVC Application
│   ├── Controllers/             # Page controllers
│   ├── Models/                  # View models
│   ├── Services/                # API client services
│   ├── Views/                   # Razor templates
│   └── wwwroot/                 # Static files (CSS, JS)
│
├── RealEstate.Api.Tests/        # API unit tests
├── RealEstate.Web.Tests/        # Web unit tests
│
├── SETUP_GUIDE.md               # Installation guide
├── DEVELOPERS_NOTES.md          # Developer documentation
└── README.md
```

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- Git
- Web browser (Chrome, Firefox, Edge)

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd RealEstate

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Running the Application

Open two terminal windows:

Terminal 1 - Start the API:

```bash
cd RealEstate.Api
dotnet run
```

Terminal 2 - Start the Web application:

```bash
cd RealEstate.Web
dotnet run
```

Access the application:

- Web Application: http://localhost:5173
- API Swagger: http://localhost:5180/swagger

### Default Admin Account

For development environment:

- Email: admin@realestate.com
- Password: Admin123!

Note: For production deployment, configure credentials via environment variables. See SETUP_GUIDE.md for details.

## API Endpoints

### Authentication

| Method | Endpoint           | Description       | Auth |
| ------ | ------------------ | ----------------- | ---- |
| POST   | /api/auth/login    | User login        | No   |
| POST   | /api/auth/register | User registration | No   |
| POST   | /api/auth/logout   | User logout       | Yes  |
| GET    | /api/auth/me       | Current user info | Yes  |

### Admin Only

| Method | Endpoint                        | Description      |
| ------ | ------------------------------- | ---------------- |
| GET    | /api/auth/users                 | List all users   |
| PUT    | /api/auth/users/{id}/role       | Update user role |
| PUT    | /api/auth/users/{id}/activate   | Activate user    |
| PUT    | /api/auth/users/{id}/deactivate | Deactivate user  |
| DELETE | /api/auth/users/{id}            | Delete user      |

### Listings

| Method | Endpoint                   | Description       |
| ------ | -------------------------- | ----------------- |
| GET    | /api/listings              | Get all listings  |
| GET    | /api/listings?type=Satilik | Filter by type    |
| GET    | /api/listings/{id}         | Get listing by ID |
| POST   | /api/listings              | Create listing    |
| PUT    | /api/listings/{id}         | Update listing    |
| DELETE | /api/listings/{id}         | Delete listing    |

## Configuration

### Environment Variables (Production)

```bash
# Required for production
export JwtSettings__SecretKey="YourSecretKeyAtLeast32Characters"
export AdminSettings__DefaultAdminEmail="admin@yourdomain.com"
export AdminSettings__DefaultAdminPassword="YourSecurePassword"
```

### API Base URL

Edit `RealEstate.Web/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5180"
  }
}
```

## Development Methodology

- Methodology: Kanban
- Project Management: Trello
- Version Control: Git/GitHub

## Documentation

- [SETUP_GUIDE.md](RealEstate/SETUP_GUIDE.md) - Detailed installation and troubleshooting
- [DEVELOPERS_NOTES.md](RealEstate/DEVELOPERS_NOTES.md) - Developer notes and best practices

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RealEstate.Api.Tests
```

## License

MIT License

## Last Updated

December 2025
