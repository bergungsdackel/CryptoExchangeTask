# MetaExchange Case Study

[![Build Status](https://github.com/bergungsdackel/CryptoExchangeTask/actions/workflows/dotnet.yml/badge.svg)](https://github.com/bergungsdackel/CryptoExchangeTask/actions)

## Overview
This repository contains a case study implementation of a meta-exchange system for cryptocurrency order execution. The solution is built with .NET Core. It includes:
- A **Domain Layer** for core business logic.
- An **Application Layer** for orchestrating.
- An **Infrastructure Layer** for data access (in this case only JSON file reading).
- Two **Presentation Layers**: a Console Application and a Web API.

## Running the Applications

### Console Application
1. Navigate to the Console Application directory:
   
   ```bash
   cd CryptoExchangeTask.Presentation.ConsoleApp
   ```

2. Run the application:
   
   ```bash
   dotnet run
   ```
   
3. Follow the on-screen prompts to enter the order type (buy/sell) and the amount of cryptocurrency.

### Web API
1. Navigate to the Web API directory:
   
   ```bash
   cd CryptoExchangeTask.Presentation.Api
   ```
2. Run the application:
   
   ```bash
   dotnet run --urls "http://localhost:8080/"
   ```
   
3. The API will be available at `http://localhost:8080`.
   
4. Use the [Swagger UI](http://localhost:8080/swagger) (enabled in development mode) to explore and test the API endpoints.

## Additional Notes
- **Domain-Driven Design:**  
  The solution is structured to emphasize separation of concerns. The Domain layer contains pure business logic, the Application layer orchestrates and transforms data, and the Infrastructure layer handles external concerns like data access.
  *Note:* Due to the limited scope of this case study, some aspects of DDD have been simplified and advanced patterns may not be fully showcased. Best practices have been applied as closely as possible within the given constraints.
  
- **Dependency Injection:**  
  Both the Console App and the Web API use built-in dependency injection for loose coupling.
  
- **Integration Testing:**  
  Integration tests are set up using `NUnit`, `Shouldly` and `WebApplicationFactory` to simulate the API environment.
