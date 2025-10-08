namespace Maman.Core.Entities;
public class BaseEntity
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; }
}
