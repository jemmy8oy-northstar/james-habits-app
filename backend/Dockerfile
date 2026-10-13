# Stage 1: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

# Copy solution and project files
COPY Balenthiran.Habits.slnx .
COPY Balenthiran.Habits.WebApi/*.csproj ./Balenthiran.Habits.WebApi/
COPY Balenthiran.Habits.Abstractions/*.csproj ./Balenthiran.Habits.Abstractions/
COPY Balenthiran.Habits.DataModels/*.csproj ./Balenthiran.Habits.DataModels/
COPY Balenthiran.Habits.DomainModels/*.csproj ./Balenthiran.Habits.DomainModels/
COPY Balenthiran.Habits.Services/*.csproj ./Balenthiran.Habits.Services/
COPY Balenthiran.Habits.EntityModels/*.csproj ./Balenthiran.Habits.EntityModels/ 
COPY Balenthiran.Habits.Database/*.csproj ./Balenthiran.Habits.Database/
COPY Balenthiran.Habits.Tests/*.csproj ./Balenthiran.Habits.Tests/
# Restore dependencies
# NOTE: `dotnet restore` with no argument restores Balenthiran.Habits.slnx, so EVERY project
# listed there must have its .csproj copied above — including test projects, which are
# not published. Adding a project to the solution without a COPY line here breaks the
# image build only, and only on a push to main, long after CI has gone green.
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish Balenthiran.Habits.WebApi/Balenthiran.Habits.WebApi.csproj -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (default ASP.NET Core port is 8080 in .NET 8+)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Balenthiran.Habits.WebApi.dll"]
