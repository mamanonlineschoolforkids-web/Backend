using Maman.Core.Entities.Auth;
using Maman.Core.Enums;
using Maman.Core.Interfaces;
using MongoDB.Driver;

namespace Maman.Infrastructure.Persistence.Repositories;
public class UserRepository : MongoRepository<User>, IUserRepository
{
	public UserRepository(MongoDbContext context) : base(context.Users)
	{
	}

	//public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.Find(u => u.Email == email && !u.IsDeleted)
	//		.FirstOrDefaultAsync(cancellationToken);
	//}

	//public async Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.Find(u => u.PhoneNumber == phone && !u.IsDeleted)
	//		.FirstOrDefaultAsync(cancellationToken);
	//}

	//public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.Find(u => u.GoogleId == googleId && !u.IsDeleted)
	//		.FirstOrDefaultAsync(cancellationToken);
	//}

	//public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
	//{
	//	return await FindAsync(u => u.Email == email && !u.IsDeleted) is not null;
		
	//	//return await _collection
	//	//	.Find(u => u.Email == email && !u.IsDeleted)
	//	//	.AnyAsync(cancellationToken);
	//}

	//public async Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.Find(u => u.PhoneNumber == phone && !u.IsDeleted)
	//		.AnyAsync(cancellationToken);
	//}

	//public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.Find(u => u.Role == role && !u.IsDeleted)
	//		.ToListAsync(cancellationToken);
	//}

	//public async Task<IReadOnlyList<User>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default)
	//{
	//	return await _collection
	//		.OfType<User>()
	//		.Find(sp => sp.ServiceProviderProfile.VerificationStatus == VerificationStatus.Pending && !sp.IsDeleted)
	//		.SortBy(sp => sp.CreatedAt)
	//		.ToListAsync(cancellationToken);
	//}

	//public async Task<IEnumerable<User>> GetDeletedUsersAsync(CancellationToken cancellationToken = default)
	//{
	//	return await _collection.Find(u => u.Status == UserStatus.Deleted).ToListAsync(cancellationToken);
	//}

	//public async Task<IEnumerable<User>> GetUsersForPermanentDeletionAsync(int daysAfterRequest, CancellationToken cancellationToken = default)
	//{
	//	var cutoffDate = DateTime.UtcNow.AddDays(-daysAfterRequest);
	//	return await _collection.Find(u =>
	//		u.Status == UserStatus.Deleted &&
	//		u.DeletionRequestedAt.HasValue &&
	//		u.DeletionRequestedAt.Value <= cutoffDate
	//	).ToListAsync(cancellationToken);
	//}

}
