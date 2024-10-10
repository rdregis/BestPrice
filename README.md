# BestPrice
REST API access crypto assets

Create Web Api Project

dotnet new webapi --use-controllers -o BestPrice
cd BestPrice
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet dev-certs https --trust
dotnet run --launch-profile https
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet tool uninstall -g dotnet-aspnet-codegenerator
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet tool update -g dotnet-aspnet-codegenerator

//dotnet aspnet-codegenerator controller -name TodoItemsController -async -api -m TodoItem -dc TodoContext -outDir Controllers
nuget xunit core
nuget xunit core.assert
nuget xunit.runner.visualstudio


Create Test Project

dotnet new mstest -o BestPriceTest
dotnet sln add BestPriceTest/BestPriceTest.csproj
dotnet add BestPriceTest/BestPriceTest.csproj reference BestPrice.csproj

dotnet new nunit -o BestPriceTestNUnit
dotnet sln add ./BestPriceTestNUnit/BestPriceTestNUnit.csproj
dotnet add ./BestPriceTestNUnit/BestPriceTestNUnit.csproj reference ./BestPrice/BestPrice.csproj  
nuget xunit 
nuget xunit core
nuget xunit.runner.visualstudio



In *.project
<PropertyGroup>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>   
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
  </PropertyGroup>

