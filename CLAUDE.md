# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core 6.0 Web API project for Import/Export document processing and management. The solution handles tax refund documents, license validations, and Excel file processing with Vietnamese localization support.

## Architecture

The project follows a layered architecture pattern:

- **ImportExport.API** - Web API layer with controllers for Excel, License, TaxRefund, and Validation
- **ImportExport.Service** - Business logic layer with services implementing domain operations
- **ImportExport.Core** - Domain models and constants
- **ImportExport.Core.CrossCutting** - Configuration settings and cross-cutting concerns
- **ImportExport.CrossCutting.Utils** - Utility helpers for Excel, Word, file operations, and text formatting

## Key Dependencies

- **DocumentFormat.OpenXml** (2.18.0) - Word document processing
- **EPPlus** (6.0.8) - Excel file manipulation
- **itext7** (8.0.0) - PDF processing
- **Spire.XLS** (12.9.2) - Excel operations
- **Humanizer** (2.14.1) with Vietnamese localization - Text humanization and formatting

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build ImportExport.API.sln

# Run the API (from ImportExpore.API directory)
dotnet run --project ImportExpore.API/ImportExport.API.csproj

# Run in development mode
dotnet run --project ImportExpore.API/ImportExport.API.csproj --environment Development
```

### Development Setup
- The API uses Swagger for documentation, available at `/swagger` in development
- Configuration is managed through `appsettings.json` and `appsettings.Development.json`
- Settings are injected via options pattern for TaxRefund, License, and GrossWeight configurations

## Core Services

- **IRefundService** - Tax refund document processing and generation
- **ILicenseService** - Product license validation and processing  
- **IValidationService** - Input validation and business rule enforcement

## File Processing

The application processes:
- Excel files for data import/export
- Word documents for tax refund templates (embedded in Templates/Refund/)
- PDF generation for official documents
- Vietnamese text formatting and number conversion

## Configuration

Settings are organized by domain:
- `TaxRefundSettings` - Tax refund processing configuration
- `LicenseSettings` - License validation rules
- `GrossWeightSettings` - Weight calculation parameters

All settings are bound from appsettings.json using the options pattern with keys defined in `AppSettingKeys`.