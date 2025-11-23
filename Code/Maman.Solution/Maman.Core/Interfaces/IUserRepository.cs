using Maman.Core.Entities.Auth;
using Maman.Core.Enums;

namespace Maman.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
	//Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	//Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
	//Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
	//Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
	//Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
	//Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
	//Task<IReadOnlyList<User>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default);
	//Task<IEnumerable<User>> GetDeletedUsersAsync(CancellationToken cancellationToken = default);
	//Task<IEnumerable<User>> GetUsersForPermanentDeletionAsync(int daysAfterRequest, CancellationToken cancellationToken = default);
}