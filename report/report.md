# Project Documentation Template

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Installation](#installation)
4. [Usage](#usage)
5. [Configuration](#configuration)
6. [Testing](#testing)
7. [Deployment](#deployment)
8. [Contributing](#contributing)
9. [License](#license)
10. [Acknowledgments](#acknowledgments)
11. [System Architecture](#systemarchitecture)

---

## Introduction
Provide a brief overview of the project, its purpose, and key features.

## Project Structure
Explain the directory structure and the purpose of each folder/file.

```
/itu-minitwit
    ├── .github/
    │   └── workflows                       # GitHub Action workflows
    │       ├── build-and-test.yml          # Automated build and test
    │       ├── build-release.yml           # Creates release on push with a tag
    │       ├── continous-deployment.yml    # Deployment to dig
    │       ├── lint-and-format-check.yml   # Automated linter and formatting checks
    │       ├── scheduled-release.yml       # Automated weekly release
    │       └── sonarcube.yml               # Automated Sonarcube checks
    ├── logging/                            # Logging configuration files
    │        ├── docker-compose.yml         # Starts ELK stack and nginx containers
    │        └── nginx.conf                 # Reverse proxy with authentication
    ├── logstash/                           # Logstash configuration
    ├── remote_files/                       # Files used remotely on the minitwit server for deployment
    ├── report/                             # Report files
    ├── src/                                # Source code
    │   ├── minitwit.core/                  # Domain Layer - Domain models
    │   ├── minitwit.infrastructure/        # Infrastructure Layer - Data access
    │   └── minitwit.web/                   # Presentation Layer - Web app & API entry point
    │       └── Program.cs                  # Program entrypoint
    ├── terraform/                          # Terraform configurations for provisioning
    │   ├── files/                          # Files used by terraform
    │   ├── modules/
    │   │   ├── minitwit_logging/           # Terraform code for logging infrastucture
    │   │   └── minitwit_server/            # Terraform code for minitwit infrastucture
    │   ├── main.tf                         # Terraform module definitions
    │   ├── terraform.tfvars                # Terraform variables
    │   └── variables.tf                    # Terraform variables declarations
    ├── tests/                              # Test cases
    │   └── minitwit.tests/
    │       ├── minitwit.tests.cs           # API tests
    │       └── playwright.test.cs          # UI tests
    │
    ├── docker-compose.yml                  # For running the program locally
    ├── Dockerfile                          # Application Dockerfile
    └── itu-minitwit.sln                    # Project solution file
```

## Installation
Step-by-step guide on how to set up the project locally.

```bash
# Clone the repository
git clone https://github.com/username/repo-name.git

# Navigate to the project directory
cd repo-name

# Install dependencies
<insert installation commands>
```

## Usage
Instructions on how to run and use the project.

```bash
# Example command to start the project
<insert usage commands>
```

## Configuration
Details about configuration files and environment variables.

## Testing
Explain how to run tests and ensure the project works as expected.

```bash
# Run tests
<insert test commands>
```

## Deployment
Steps to deploy the project to production.

## Contributing
Guidelines for contributing to the project.

## License
This Project Itu_Minitwit is licensed and distributed under the MIT license

## Acknowledgments
Credit individuals or resources that helped in the project.


## SystemArchitecture
```
# Dependency List:
1. Microsoft.EntityFrameworkCore.Design - Version: 9.0.1
2. Microsoft.Extensions.Configuration - Version: 9.0.2
3. Microsoft.Extensions.Configuration.EnvironmentVariables - Version: 9.0.2
4. Microsoft.Extensions.Configuration.UserSecrets - Version: 9.0.2
5. Npgsql.EntityFrameworkCore.PostgreSQL - Version: 9.0.4
6. prometheus-net - Version: 8.2.1
7. Serilog.AspNetCore - Version: 9.0.0
8. Serilog.Sinks.Console - Version: 6.0.0
9. Microsoft.AspNetCore.Identity - Version: 2.3.1
10. Microsoft.EntityFrameworkCore.Sqlite - Version: 9.0.1
11. Microsoft.AspNetCore.Identity.EntityFrameworkCore - Version: 9.0.1
12. Microsoft.AspNetCore.Identity.UI - Version: 9.0.1
13. Microsoft.EntityFrameworkCore.Tools - Version: 9.0.0
14. Microsoft.VisualStudio.Web.CodeGeneration.Design - Version: 9.0.0
15. prometheus-net.AspNetCore - Version: 8.2.1
16. Serilog - Version: 4.2.0
17. Serilog.Formatting.Compact - Version: 3.0.0
18. Serilog.Sinks.Elasticsearch - Version: 10.0.0
19. Serilog.Sinks.Network - Version: 2.0.2.68
20. Serilog.Sinks.Async - Version: 1.5.0
21. coverlet.collector - Version: 6.0.4
22. Microsoft.AspNetCore.Mvc.Testing - Version: 9.0.2
23. Microsoft.NET.Test.Sdk - Version: 17.13.0
24. Microsoft.Playwright.NUnit - Version: 1.50.0
25. xunit - Version: 2.9.2
26. xunit.runner.visualstudio - Version: 3.0.0
27. Postgres - Version: 16.9
28. Kibana - Version: 8.12.1
29. logstash - Version: 8.12.1
30. elasticsearch - Version: 8.12.1
31. Nginx - Version: 1.27.0
32. Dotnet_SDK - Version: 9.0.0
33. org.Sonarcube - Version: 6.1.0
```