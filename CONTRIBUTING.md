# 🤝 Contributing to TicketManagement System

Thank you for your interest in contributing to the TicketManagement System! This document provides guidelines and instructions for contributing to this project.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Git Workflow](#git-workflow)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Guidelines](#issue-guidelines)
- [Documentation](#documentation)

---

## 📜 Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inspiring community for all. Please be respectful and constructive in all interactions.

### Our Standards

**Positive behavior includes:**
- Using welcoming and inclusive language
- Being respectful of differing viewpoints
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

**Unacceptable behavior includes:**
- Harassment, discriminatory jokes or language
- Personal or political attacks
- Public or private harassment
- Publishing others' private information without permission

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **Required**:
  - [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
  - [Docker](https://www.docker.com/get-started) and Docker Compose
  - [Git](https://git-scm.com/downloads)
  - [PostgreSQL](https://www.postgresql.org/download/) (or use Docker)
  - [Redis](https://redis.io/download) (or use Docker)

- **Recommended**:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio Code](https://code.visualstudio.com/) with C# extension
  - [Postman](https://www.postman.com/) or similar API testing tool

### First Time Setup

1. **Fork the repository**
   ```bash
   # Go to https://github.com/TrungHoang2003/TicketManagement
   # Click the 'Fork' button
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/TicketManagement.git
   cd TicketManagement
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/TrungHoang2003/TicketManagement.git
   ```

4. **Verify remotes**
   ```bash
   git remote -v
   # Should show both 'origin' (your fork) and 'upstream' (original repo)
   ```

---

## 🛠️ Development Setup

### 1. Environment Configuration

Create environment files from templates:

```bash
# Development environment
cp .env.example .env

# Docker environment
cp .env.docker.example .env.docker
```

Edit `.env` with your local settings:

```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=ticketDb;Username=root;Password=root

# Redis
ConnectionStrings__Redis=localhost:6379

# JWT Settings
JWT__Secret=your-super-secret-key-min-32-characters-long
JWT__Issuer=TicketManagement
JWT__Audience=TicketManagement
JWT__ExpirationMinutes=60

# Google OAuth
GoogleOAuth__ClientId=your-google-client-id
GoogleOAuth__ClientSecret=your-google-client-secret

# Cloudinary
Cloudinary__CloudName=your-cloud-name
Cloudinary__ApiKey=your-api-key
Cloudinary__ApiSecret=your-api-secret
```

### 2. Database Setup

#### Option A: Using Docker (Recommended)

```bash
# Start only PostgreSQL and Redis
docker-compose up postgres redis -d

# Wait for services to be ready
docker-compose ps
```

#### Option B: Local Installation

Install PostgreSQL and Redis locally, then configure connection strings accordingly.

### 3. Database Migration

```bash
# Apply migrations
dotnet ef database update --project Infrastructure --startup-project TicketManagement.Api

# Or create a new migration if needed
dotnet ef migrations add YourMigrationName --project Infrastructure --startup-project TicketManagement.Api
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run the Application

```bash
# Run API
dotnet run --project TicketManagement.Api

# Or use watch mode for hot reload
dotnet watch --project TicketManagement.Api
```

The API will be available at:
- HTTP: `http://localhost:5105`
- Swagger: `http://localhost:5105/swagger`
- Scalar: `http://localhost:5105/scalar/v1`

### 7. Run Tests (When Available)

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TicketManagement.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 📁 Project Structure

```
TicketManagement/
├── Domain/                         # Core business logic layer
│   ├── Entities/                  # Domain entities
│   └── Interfaces/                # Domain interfaces
├── Application/                    # Application logic layer
│   ├── Services/                  # Business services
│   ├── DTOs/                      # Data transfer objects
│   ├── Mappings/                  # AutoMapper profiles
│   └── Errors/                    # Error definitions
├── Infrastructure/                 # Infrastructure layer
│   ├── Repositories/              # Data access implementations
│   ├── Database/                  # DbContext and configurations
│   ├── Background/                # Background job services
│   └── Migrations/                # EF Core migrations
├── TicketManagement.Api/          # Presentation layer
│   ├── Controllers/               # API controllers
│   └── Middlewares/               # Custom middlewares
├── BuildingBlocks/                # Shared components
│   ├── Commons/                   # Common utilities
│   ├── Settings/                  # Configuration classes
│   └── EmailHelper/               # Email utilities
└── tests/                         # Test projects (to be added)
    ├── UnitTests/
    ├── IntegrationTests/
    └── E2ETests/
```

### Layer Responsibilities

- **Domain**: Pure business logic, no external dependencies
- **Application**: Use cases, orchestration, business workflows
- **Infrastructure**: Database, external services, file system
- **API**: HTTP concerns, request/response handling
- **BuildingBlocks**: Shared code across layers

---

## 📝 Coding Standards

### General Guidelines

1. **Follow Clean Code Principles**
   - Write self-documenting code
   - Keep methods small and focused (< 20 lines preferred)
   - Use meaningful names
   - Avoid deep nesting (max 3 levels)
   - Don't repeat yourself (DRY)

2. **SOLID Principles**
   - Single Responsibility Principle
   - Open/Closed Principle
   - Liskov Substitution Principle
   - Interface Segregation Principle
   - Dependency Inversion Principle

### C# Coding Conventions

#### Naming Conventions

```csharp
// ✅ Good
public class TicketService { }              // PascalCase for classes
public interface ITicketRepository { }      // PascalCase with 'I' prefix
public async Task<Result> CreateAsync() { } // PascalCase for methods
private readonly IUnitOfWork _unitOfWork;   // camelCase with underscore prefix
public string Title { get; set; }           // PascalCase for properties
const int MaxRetries = 3;                   // PascalCase for constants

// ❌ Bad
public class ticketservice { }              // Wrong casing
public interface TicketRepository { }       // Missing 'I' prefix
private IUnitOfWork unitOfWork;            // Missing underscore
```

#### Code Structure

```csharp
// ✅ Good: Clear, focused method
public async Task<Result> CreateTicket(CreateTicketDto dto)
{
    var validation = await ValidateAsync(dto);
    if (!validation.Success)
        return validation;
    
    var ticket = MapToEntity(dto);
    await _repository.AddAsync(ticket);
    await _unitOfWork.SaveChangesAsync();
    
    await NotifyStakeholders(ticket);
    
    return Result.IsSuccess();
}

// ❌ Bad: Too many responsibilities
public async Task<Result> CreateTicket(CreateTicketDto dto)
{
    // 100+ lines of validation, mapping, business logic, notifications, logging...
}
```

#### Async/Await

```csharp
// ✅ Good: Properly async
public async Task<User> GetUserAsync(int id)
{
    return await _context.Users
        .Include(u => u.Department)
        .FirstOrDefaultAsync(u => u.Id == id);
}

// ❌ Bad: Fake async
public async Task<User> GetUser(int id)
{
    return _context.Users.FirstOrDefault(u => u.Id == id); // CS1998 warning
}

// ✅ Good: Synchronous when appropriate
public User GetUserById(int id)
{
    return _cache.Get<User>(id);
}
```

#### Null Handling

```csharp
// ✅ Good: Nullable reference types
public class User
{
    public required string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public Department? Department { get; set; }
}

// ❌ Bad: Non-nullable without initialization
public class User
{
    public string FullName { get; set; } // CS8618 warning
}
```

#### Error Handling

```csharp
// ✅ Good: Specific exceptions
public async Task<Result> ProcessTicket(int id)
{
    var ticket = await _repository.GetByIdAsync(id);
    if (ticket == null)
        return Result.Failure(new Error("NOT_FOUND", $"Ticket {id} not found"));
    
    try
    {
        await ProcessTicketLogic(ticket);
        return Result.IsSuccess();
    }
    catch (BusinessException ex)
    {
        return Result.Failure(new Error("BUSINESS_ERROR", ex.Message));
    }
}

// ❌ Bad: Generic exceptions and swallowing
public void ProcessTicket(int id)
{
    try
    {
        // logic
    }
    catch (Exception) // Too broad
    {
        // Swallowing exception
    }
}
```

### Code Organization

```csharp
// Class organization order:
public class TicketService : ITicketService
{
    // 1. Constants
    private const int MaxRetries = 3;
    
    // 2. Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketService> _logger;
    
    // 3. Constructor
    public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    // 4. Public methods
    public async Task<Result> CreateAsync(CreateTicketDto dto)
    {
        // ...
    }
    
    // 5. Private methods
    private async Task<bool> ValidateAsync(CreateTicketDto dto)
    {
        // ...
    }
}
```

### Comments and Documentation

```csharp
// ✅ Good: XML documentation for public APIs
/// <summary>
/// Creates a new ticket and assigns it to the appropriate department head.
/// </summary>
/// <param name="dto">Ticket creation data including title, content, and priority.</param>
/// <returns>Result indicating success or failure with error details.</returns>
/// <exception cref="ValidationException">Thrown when input validation fails.</exception>
public async Task<Result> CreateAsync(CreateTicketDto dto)
{
    // Complex logic explanation when needed
    // We check category first because it determines department routing
    var category = await _repository.GetCategoryAsync(dto.CategoryId);
    
    // ...
}

// ❌ Bad: Obvious comments
// Get user by id
var user = await GetUserAsync(id);

// ❌ Bad: No documentation on public API
public async Task<Result> CreateAsync(CreateTicketDto dto)
```

### Testing Standards

```csharp
// ✅ Good: Clear, AAA pattern
[Fact]
public async Task CreateTicket_WithValidData_ShouldSucceed()
{
    // Arrange
    var dto = new CreateTicketDto
    {
        Title = "Test Ticket",
        Content = "Test Content",
        Priority = "High"
    };
    _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Ticket>()))
        .ReturnsAsync(true);
    
    // Act
    var result = await _sut.CreateAsync(dto);
    
    // Assert
    Assert.True(result.Success);
    _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Once);
}

// Test naming: MethodName_Scenario_ExpectedBehavior
```

---

## 🔀 Git Workflow

### Branch Naming

```bash
# Feature branches
feature/ticket-assignment
feature/add-email-notifications

# Bug fix branches
bugfix/null-reference-in-ticket-service
bugfix/authentication-token-expiry

# Hotfix branches (critical production issues)
hotfix/security-vulnerability-fix

# Refactor branches
refactor/improve-repository-pattern
refactor/extract-notification-service
```

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Format: <type>(<scope>): <subject>

# Examples:
feat(ticket): add ticket assignment functionality
fix(auth): resolve JWT token expiration issue
docs(readme): update installation instructions
style(format): apply consistent code formatting
refactor(service): extract email notification logic
test(ticket): add unit tests for ticket creation
chore(deps): update Entity Framework Core to 9.0

# Breaking changes:
feat(api)!: change ticket creation endpoint structure

BREAKING CHANGE: The ticket creation endpoint now requires 'categoryId' instead of 'categoryName'
```

### Commit Best Practices

```bash
# ✅ Good: Small, focused commits
git commit -m "feat(ticket): add title validation"
git commit -m "feat(ticket): add content length check"
git commit -m "test(ticket): add validation tests"

# ❌ Bad: Large, unfocused commits
git commit -m "various updates and fixes"
git commit -m "WIP"
```

### Workflow Steps

1. **Update your fork**
   ```bash
   git checkout main
   git fetch upstream
   git merge upstream/main
   git push origin main
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   ```bash
   # Make changes, commit frequently
   git add .
   git commit -m "feat(scope): description"
   ```

4. **Keep your branch updated**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

5. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create Pull Request**
   - Go to GitHub
   - Click "New Pull Request"
   - Fill in the PR template

---

## 🧪 Testing Guidelines

### Test Structure

```
tests/
├── TicketManagement.UnitTests/
│   ├── Services/
│   │   ├── TicketServiceTests.cs
│   │   └── UserServiceTests.cs
│   ├── Domain/
│   │   └── TicketEntityTests.cs
│   └── Common/
│       └── ResultPatternTests.cs
├── TicketManagement.IntegrationTests/
│   ├── Controllers/
│   │   └── TicketControllerTests.cs
│   └── Repositories/
│       └── TicketRepositoryTests.cs
└── TicketManagement.E2ETests/
    └── Scenarios/
        └── TicketWorkflowTests.cs
```

### Writing Tests

**Unit Test Example:**
```csharp
public class TicketServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailBackgroundService> _emailServiceMock;
    private readonly TicketService _sut; // System Under Test
    
    public TicketServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailBackgroundService>();
        
        // Setup common mocks
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        _sut = new TicketService(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
    }
    
    [Fact]
    public async Task CreateTicket_WithValidData_ShouldSucceed()
    {
        // Arrange
        var dto = CreateValidTicketDto();
        
        // Act
        var result = await _sut.CreateAsync(dto);
        
        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTicket_WithInvalidTitle_ShouldFail(string invalidTitle)
    {
        // Arrange
        var dto = CreateValidTicketDto();
        dto.Title = invalidTitle;
        
        // Act
        var result = await _sut.CreateAsync(dto);
        
        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("VALIDATION_ERROR", result.Error.Code);
    }
    
    private CreateTicketDto CreateValidTicketDto() => new()
    {
        Title = "Test Ticket",
        Content = "Test Content",
        Priority = "High",
        CategoryId = 1
    };
    
    public void Dispose()
    {
        // Cleanup if needed
    }
}
```

### Test Coverage Requirements

- **Minimum**: 80% code coverage
- **Critical Paths**: 100% coverage (authentication, authorization, payment, etc.)
- **New Features**: Must include tests before merging

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~TicketServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true

# Watch mode (auto-run on changes)
dotnet watch test
```

---

## 📬 Pull Request Process

### Before Creating PR

**Checklist:**
- [ ] Code compiles without errors
- [ ] All tests pass
- [ ] No new compiler warnings
- [ ] Code follows style guidelines
- [ ] Added/updated tests for new functionality
- [ ] Updated documentation if needed
- [ ] Ran code formatter
- [ ] Self-reviewed the changes
- [ ] No sensitive information committed

### PR Template

```markdown
## Description
Brief description of the changes and why they were made.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Related Issues
Fixes #(issue number)
Relates to #(issue number)

## How Has This Been Tested?
Describe the tests you ran and how to reproduce them.

- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Screenshots (if applicable)
Add screenshots to help explain your changes.

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where necessary
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally
- [ ] Any dependent changes have been merged
```

### Review Process

1. **Automated Checks**: CI/CD pipeline runs automatically
2. **Code Review**: At least one maintainer must approve
3. **Address Feedback**: Make requested changes
4. **Final Approval**: Maintainer will merge when ready

### PR Best Practices

- Keep PRs small (< 400 lines changed preferred)
- One feature/fix per PR
- Respond to reviews promptly
- Be open to feedback
- Update your PR branch if main has changed

---

## 🐛 Issue Guidelines

### Creating Issues

**Bug Report Template:**
```markdown
## Bug Description
A clear and concise description of what the bug is.

## Steps to Reproduce
1. Go to '...'
2. Click on '...'
3. See error

## Expected Behavior
What you expected to happen.

## Actual Behavior
What actually happened.

## Screenshots
If applicable, add screenshots.

## Environment
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 9.0]
- Browser (if applicable): [e.g. Chrome 120]

## Additional Context
Any other context about the problem.
```

**Feature Request Template:**
```markdown
## Feature Description
A clear description of the feature you'd like to see.

## Use Case
Explain why this feature would be useful.

## Proposed Solution
How you think it should work.

## Alternatives Considered
Any alternative solutions you've thought about.

## Additional Context
Any other context or screenshots.
```

---

## 📚 Documentation

### Code Documentation

- Add XML documentation to all public APIs
- Document complex business logic
- Keep comments up-to-date with code changes
- Use meaningful variable and method names

### README Updates

- Update README.md when adding new features
- Keep installation instructions current
- Document new environment variables
- Update architecture diagrams if structure changes

### API Documentation

- Swagger/OpenAPI annotations on controllers
- Document all endpoints, parameters, and responses
- Provide example requests/responses
- Note any breaking changes

---

## 🔍 Code Review Guidelines

### For Authors

- Keep changes focused and small
- Provide context in PR description
- Respond professionally to feedback
- Don't take feedback personally
- Mark conversations as resolved when addressed

### For Reviewers

- Be constructive and respectful
- Explain the "why" behind suggestions
- Approve if changes are acceptable even if not perfect
- Block PR only for serious issues
- Acknowledge good work

### Review Checklist

**Functionality:**
- [ ] Code does what it's supposed to do
- [ ] Edge cases are handled
- [ ] No obvious bugs

**Design:**
- [ ] Follows architecture patterns
- [ ] Appropriate design patterns used
- [ ] No unnecessary complexity

**Code Quality:**
- [ ] Readable and maintainable
- [ ] Follows coding standards
- [ ] No code duplication
- [ ] Proper error handling

**Tests:**
- [ ] Adequate test coverage
- [ ] Tests are meaningful
- [ ] Tests pass

**Documentation:**
- [ ] Code is self-documenting or commented
- [ ] Public APIs documented
- [ ] README updated if needed

---

## 🎯 Development Tips

### Useful Commands

```bash
# Format code
dotnet format

# Clean solution
dotnet clean

# Watch for changes (hot reload)
dotnet watch run --project TicketManagement.Api

# List outdated packages
dotnet list package --outdated

# Update package
dotnet add package PackageName

# Entity Framework commands
dotnet ef migrations add MigrationName --project Infrastructure
dotnet ef database update --project Infrastructure
dotnet ef migrations remove --project Infrastructure

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

### Debugging Tips

1. **Use logging effectively**
   ```csharp
   _logger.LogInformation("Creating ticket: {Title}", dto.Title);
   _logger.LogWarning("User {UserId} not found", userId);
   _logger.LogError(ex, "Failed to create ticket");
   ```

2. **Use breakpoints and conditional breakpoints**

3. **Check Swagger for API testing**

4. **Use Postman collections for complex scenarios**

---

## ❓ Getting Help

### Resources

- **Documentation**: Check README.md and CODE_REVIEW.md
- **Issues**: Search existing issues before creating new ones
- **Discussions**: Use GitHub Discussions for questions
- **Email**: Contact maintainers at hoangtrung2003@example.com

### Common Issues

**Problem**: Can't connect to database
```bash
# Solution: Check if PostgreSQL is running
docker-compose ps postgres

# Verify connection string in .env
ConnectionStrings__DefaultConnection=Host=localhost;Database=ticketDb;...
```

**Problem**: JWT token errors
```bash
# Solution: Check JWT configuration
JWT__Secret=your-secret-key-must-be-at-least-32-characters-long
```

**Problem**: Build errors after pulling latest
```bash
# Solution: Clean and restore
dotnet clean
dotnet restore
dotnet build
```

---

## 📄 License

By contributing to TicketManagement, you agree that your contributions will be licensed under the MIT License.

---

## 🙏 Thank You!

Thank you for contributing to TicketManagement! Your efforts help make this project better for everyone.

**Happy Coding! 🚀**
