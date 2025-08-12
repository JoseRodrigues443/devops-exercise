.PHONY: help setup build run test clean docker db-setup db-migrate db-reset lint format

# Default target
help: ## Show this help message
	@echo "Five9 Coding Assessment - Available commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo ""
	@echo "Environment:"
	@echo "  ASPNETCORE_ENVIRONMENT: $(ASPNETCORE_ENVIRONMENT)"
	@echo "  Connection String: Host=localhost;Database=agentstate;Username=gitpod;Password=gitpod"

setup:
	@echo "Setting up Five9 .NET project..."
	dotnet new sln -n AgentStateAPI
	dotnet new webapi -n AgentStateAPI -f net8.0
	dotnet sln add AgentStateAPI/AgentStateAPI.csproj
	cd AgentStateAPI && \
	dotnet add package Microsoft.EntityFrameworkCore.Design && \
	dotnet add package Microsoft.EntityFrameworkCore.Tools && \
	dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL && \
	dotnet add package Swashbuckle.AspNetCore
	dotnet new xunit -n AgentStateAPI.Tests
	dotnet add AgentStateAPI.Tests/AgentStateAPI.Tests.csproj reference AgentStateAPI/AgentStateAPI.csproj
	dotnet sln add AgentStateAPI.Tests/AgentStateAPI.Tests.csproj
	cd AgentStateAPI.Tests && \
	dotnet add package Microsoft.EntityFrameworkCore.InMemory && \
	dotnet add package Moq
	dotnet restore
	@echo "Project setup complete!"

install:
	@echo "Restoring packages..."
	dotnet restore

build:
	@echo "🔨 Building solution..."
	dotnet build

build-release:
	@echo "🔨 Building solution (Release)..."
	dotnet build --configuration Release

run:
	@echo "Starting API with hot reload..."
	cd AgentStateAPI && dotnet watch run

run-no-watch:
	@echo "Starting API..."
	cd AgentStateAPI && dotnet run

dev: install build run

# Testing
test:
	@echo "Running tests..."
	dotnet test

test-verbose:
	@echo "Running tests (verbose)..."
	dotnet test --verbosity normal

test-coverage:
	@echo "Running tests with coverage..."
	dotnet test --collect:"XPlat Code Coverage"

# Database operations
db-check:
	@echo "Checking PostgreSQL status..."
	sudo service postgresql status || echo "PostgreSQL not running"

db-start:
	@echo "Starting PostgreSQL..."
	sudo service postgresql start
	@echo "PostgreSQL started"

db-stop:
	@echo "Stopping PostgreSQL..."
	sudo service postgresql stop

db-setup: db-start
	@echo "Setting up database..."
	sudo -u postgres createdb agentstate || echo "Database already exists"
	sudo -u postgres psql -c "CREATE USER gitpod WITH PASSWORD 'gitpod';" || echo "User already exists"
	sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE agentstate TO gitpod;"
	sudo -u postgres psql -c "ALTER USER gitpod CREATEDB;"
	@echo "Database setup complete"

db-migrate:
	@echo "Creating and applying migrations..."
	cd AgentStateAPI && \
	dotnet ef migrations add InitialCreate --force && \
	dotnet ef database update
	@echo "Migrations applied"

db-reset:
	@echo "Resetting database..."
	sudo -u postgres dropdb agentstate || echo "Database doesn't exist"
	sudo -u postgres createdb agentstate
	sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE agentstate TO gitpod;"
	cd AgentStateAPI && dotnet ef database update
	@echo "Database reset complete"

db-connect:
	@echo "🐘 Connecting to database..."
	psql -h localhost -U gitpod -d agentstate

# Code quality
lint: 
	@echo "🔍 Checking code formatting..."
	dotnet format --verify-no-changes

format:
	@echo "✨ Formatting code..."
	dotnet format

# Docker operations
docker-build: ## Build Docker image
	@echo "Building Docker image..."
	docker build -t agentstate-api .

docker-run:
	@echo "Running Docker container..."
	docker run -p 8080:8080 --env-file .env agentstate-api

docker-compose-up:
	@echo "Starting services with docker-compose..."
	docker-compose up -d

docker-compose-down:
	@echo "Stopping docker-compose services..."
	docker-compose down

# Cleanup
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf AgentStateAPI/bin AgentStateAPI/obj
	rm -rf AgentStateAPI.Tests/bin AgentStateAPI.Tests/obj

clean-all: clean
	@echo "Cleaning everything..."
	rm -rf ~/.nuget/packages
	dotnet nuget locals all --clear

# API testing
test-api:
	@echo "Testing API endpoint..."
	curl -X POST "http://localhost:5000/api/agentstate/events" \
		-H "Content-Type: application/json" \
		-d '{ \
			"agentId": "6B29FC40-CA47-1067-B31D-00DD010662DA", \
			"agentName": "john smith", \
			"timestampUtc": "2023-04-23T18:25:43.511Z", \
			"action": "CALL_STARTED", \
			"queueIds": ["3a8cc33a-3f09-4ce5-9c53-e94585a410c8"] \
		}'

test-api-lunch: ## Test lunch time scenario
	@echo "Testing lunch time scenario..."
	curl -X POST "http://localhost:5000/api/agentstate/events" \
		-H "Content-Type: application/json" \
		-d '{ \
			"agentId": "6B29FC40-CA47-1067-B31D-00DD010662DA", \
			"agentName": "lunch agent", \
			"timestampUtc": "2023-04-23T12:00:00.000Z", \
			"action": "START_DO_NOT_DISTURB", \
			"queueIds": [] \
		}'

test-api-late: ## Test late event (should fail)
	@echo "Testing late event scenario..."
	curl -X POST "http://localhost:5000/api/agentstate/events" \
		-H "Content-Type: application/json" \
		-d '{ \
			"agentId": "6B29FC40-CA47-1067-B31D-00DD010662DA", \
			"agentName": "late agent", \
			"timestampUtc": "2023-04-23T10:00:00.000Z", \
			"action": "CALL_STARTED", \
			"queueIds": [] \
		}'

# Health checks
health: ## Check API health
	@echo "Checking API health..."
	curl -f http://localhost:5000/health || echo "API not responding"


# Full workflow targets
init: setup db-setup
	@echo " Project initialization complete!"
	@echo "Run 'make dev' to start development"

# Development workflow
dev-flow: install build db-migrate run

# Monitoring and logs
logs:
	docker-compose logs -f api

# Default environment variables
ifndef ASPNETCORE_ENVIRONMENT
ASPNETCORE_ENVIRONMENT := Development
endif

export ASPNETCORE_ENVIRONMENT