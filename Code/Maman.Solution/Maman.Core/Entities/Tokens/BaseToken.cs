using Maman.Core.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Core.Entities.Tokens;

public class BaseToken : BaseEntity
{
	[BsonRepresentation(BsonType.ObjectId)]

	public string UserId { get; set; }
	public string Token { get; set; }
	public DateTime ExpiresAt { get; set; }

	public bool IsUsed { get; set; }
	public DateTime UsedAt { get; set; }

}
