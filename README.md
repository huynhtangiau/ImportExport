# Import/Export Document Processing API

A comprehensive ASP.NET Core 6.0 Web API for processing import/export documents, tax refunds, and license validations with Vietnamese localization support.

## Features

- **Tax Refund Processing** - Generate and process tax refund documents with Vietnamese formatting
- **License Management** - Product license validation and processing
- **Excel Operations** - Import/export data through Excel files with advanced formatting
- **Document Generation** - Create Word and PDF documents from templates
- **Validation Services** - Business rule enforcement and input validation
- **Semantic Search** - PDF file semantic search capabilities

## Architecture

The solution follows a clean layered architecture:

```
├── ImportExport.API/              # Web API controllers and endpoints
├── ImportExport.Service/          # Business logic and services
├── ImportExport.Core/             # Domain models and constants
├── ImportExport.Core.CrossCutting/# Configuration and settings
└── ImportExport.CrossCutting.Utils/# Utility helpers
```

## Key Technologies

- **ASP.NET Core 6.0** - Web API framework
- **DocumentFormat.OpenXml** - Word document processing
- **EPPlus** - Excel file manipulation
- **iText7** - PDF generation and processing
- **Spire.XLS** - Advanced Excel operations
- **Humanizer** - Vietnamese text formatting and number conversion

## Getting Started

### Prerequisites

- .NET 6.0 SDK
- Visual Studio 2022 or VS Code

### Running the Application

```bash
# Clone the repository
git clone <repository-url>

# Navigate to project directory
cd ImportExport

# Build the solution
dotnet build ImportExport.API.sln

# Run the API
dotnet run --project ImportExpore.API/ImportExport.API.csproj
```

The API will be available at `https://localhost:5001` with Swagger documentation at `/swagger`.

## API Endpoints

- **Excel Controller** - Data import/export operations
- **License Controller** - Product license management
- **Tax Refund Controller** - Tax refund document processing
- **Validation Controller** - Input validation services

## Configuration

Application settings are managed through:
- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development overrides

Key configuration sections:
- `TaxRefundSettings` - Tax processing parameters
- `LicenseSettings` - License validation rules
- `GrossWeightSettings` - Weight calculation settings

## File Processing

Supports processing of:
- Excel files (.xlsx, .xls) for data operations
- Word documents (.docx) for template processing
- PDF files for document generation and semantic search
- Vietnamese text with proper formatting and number conversion

## Development

The project uses:
- Dependency injection for service registration
- Options pattern for configuration binding
- Repository pattern for data access
- Vietnamese localization with Humanizer

## License

[Add your license information here]