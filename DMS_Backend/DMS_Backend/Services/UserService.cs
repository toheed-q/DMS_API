using DMS.Models;
using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Data;
using DMS_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS_Backend.Services
{
    /// <summary>
    /// Creates and lists logins. A Salesman-role account MUST be linked to a
    /// Salesman record — that link is what scopes the mobile user to their own data.
    /// </summary>
    public class UserService : IUserService
    {
        private static readonly string[] ValidRoles = [Roles.Admin, Roles.User, Roles.Salesman];

        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext db,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUser,
            ILogger<UserService> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request)
        {
            var role = ValidRoles.FirstOrDefault(r =>
                string.Equals(r, request.Role, StringComparison.OrdinalIgnoreCase));

            if (role is null)
                return Fail($"Role must be one of: {string.Join(", ", ValidRoles)}.", ErrorType.Validation);

            var username = request.Username.Trim();

            if (await _db.Users.AnyAsync(u => u.Username == username))
                return Fail($"Username '{username}' is already taken.", ErrorType.Conflict);

            // A salesman login is meaningless without the salesman record it maps to.
            // Either link to an existing record, or create one here so that setting up
            // a new salesman is a single call instead of two separate inserts.
            string? salesmanName = null;
            int? salesmanId = null;
            Salesman? newSalesman = null;

            if (role == Roles.Salesman)
            {
                if (request.SalesmanId is not null)
                {
                    salesmanName = await _db.Salesmen
                        .Where(s => s.SalesmanId == request.SalesmanId.Value)
                        .Select(s => s.FullName)
                        .FirstOrDefaultAsync();

                    if (salesmanName is null)
                        return Fail($"Salesman {request.SalesmanId} was not found.", ErrorType.NotFound);

                    if (await _db.Users.AnyAsync(u => u.SalesmanId == request.SalesmanId.Value))
                        return Fail($"Salesman {request.SalesmanId} already has a login.", ErrorType.Conflict);

                    salesmanId = request.SalesmanId;
                }
                else
                {
                    // No existing record named — create one from the request.
                    if (string.IsNullOrWhiteSpace(request.FullName))
                        return Fail("fullName is required to create a salesman.", ErrorType.Validation);

                    if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                        return Fail("phoneNumber is required to create a salesman.", ErrorType.Validation);

                    newSalesman = new Salesman
                    {
                        FullName = request.FullName.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        CNIC = string.IsNullOrWhiteSpace(request.CNIC) ? null : request.CNIC.Trim(),
                        MonthlySalary = request.MonthlySalary,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _db.Salesmen.Add(newSalesman);
                    salesmanName = newSalesman.FullName;
                }
            }

            // The salesman record has to be saved first to get its identity, but the
            // login is what makes it useful — so both go in one transaction and a
            // failure on either side leaves nothing behind.
            await using var tx = await _db.Database.BeginTransactionAsync();

            if (newSalesman is not null)
            {
                await _db.SaveChangesAsync();
                salesmanId = newSalesman.SalesmanId;
            }

            var isAdmin = role == Roles.Admin;

            var user = new User
            {
                Username = username,
                Password = _passwordHasher.Hash(request.Password),
                Role = role,
                SalesmanId = salesmanId,
                FullName = request.FullName,
                Email = request.Email,
                IsApproved = true,
                IsAccountActive = request.IsAccountActive,
                CreatedAt = DateTime.UtcNow,
                ApprovedBy = _currentUser.UserId,
                ApprovedAt = DateTime.UtcNow,

                // Admins implicitly have everything; others get exactly what was granted.
                CanAccessPOS = isAdmin || request.CanAccessPOS,
                CanAccessProducts = isAdmin || request.CanAccessProducts,
                CanAccessStockViewer = isAdmin || request.CanAccessStockViewer,
                CanAccessShops = isAdmin || request.CanAccessShops,
                CanAccessRoutes = isAdmin || request.CanAccessRoutes,
                CanAccessCategories = isAdmin || request.CanAccessCategories,
                CanAccessBillsHistory = isAdmin || request.CanAccessBillsHistory,
                CanAccessEmployees = isAdmin || request.CanAccessEmployees,
                CanAccessCompanies = isAdmin || request.CanAccessCompanies
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation(
                "User {Username} created with role {Role} (salesman {SalesmanId}, newly created: {Created}) by {Creator}",
                user.Username, user.Role, user.SalesmanId, newSalesman is not null, _currentUser.Username);

            return Result<UserDto>.Success(Map(user, salesmanName));
        }

        public async Task<Result<PagedResult<UserDto>>> GetUsersAsync(PaginationQuery query)
        {
            var users = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                users = users.Where(u => u.Username.Contains(term) ||
                                         (u.FullName != null && u.FullName.Contains(term)));
            }

            var totalCount = await users.CountAsync();

            var rows = await users
                .OrderBy(u => u.Username)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new
                {
                    User = u,
                    SalesmanName = _db.Salesmen
                        .Where(s => s.SalesmanId == u.SalesmanId)
                        .Select(s => s.FullName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var items = rows.Select(r => Map(r.User, r.SalesmanName)).ToList();

            return Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }

        private static UserDto Map(User user, string? salesmanName)
        {
            var permissions = new List<string>();
            void Add(bool granted, string permission) { if (granted) permissions.Add(permission); }

            Add(user.CanAccessPOS, Permissions.POS);
            Add(user.CanAccessProducts, Permissions.Products);
            Add(user.CanAccessStockViewer, Permissions.StockViewer);
            Add(user.CanAccessShops, Permissions.Shops);
            Add(user.CanAccessRoutes, Permissions.Routes);
            Add(user.CanAccessCategories, Permissions.Categories);
            Add(user.CanAccessBillsHistory, Permissions.BillsHistory);
            Add(user.CanAccessEmployees, Permissions.Employees);
            Add(user.CanAccessCompanies, Permissions.Companies);

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                SalesmanId = user.SalesmanId,
                SalesmanName = salesmanName,
                FullName = user.FullName,
                Email = user.Email,
                IsApproved = user.IsApproved,
                IsAccountActive = user.IsAccountActive == true,
                Permissions = permissions
            };
        }

        private static Result<UserDto> Fail(string message, ErrorType type)
            => Result<UserDto>.Failure(message, type);
    }
}
