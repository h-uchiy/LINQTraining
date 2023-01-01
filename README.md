# LINQTraining

Code samples for LINQ to Object / LINQ to Entity training.

(Code comments and documents are written in Japanese.)

## Goals

* Learn effective LINQ to Object / LINQ to Entity
* Learn how to implement custom query method.
* Learn how to implement dynamic query for LINQ to Entity.

## Getting Started

### Dependencies

* IDE (Visual Studio or Rider)
* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
* [EF Core command-line tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
* [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)

### Installing / Executing program

* download or clone this source code
* run this command at `LINQTraining.csproj` existing folder

```bat
dotnet tool install --version 3.1.31 --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Code samples are implemented as xUnit tests.
