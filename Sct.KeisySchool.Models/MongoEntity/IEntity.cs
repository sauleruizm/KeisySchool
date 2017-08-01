using MongoDB.Bson.Serialization.Attributes;

namespace Sct.KeisySchool.Models.MongoEntity
{
    /// <summary>
    /// Generic Entity Interface
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Represent the identifier in mogodb entity
        /// </summary>
        [BsonId]
        TKey Id { get; set; }
    }

    /// <summary>
    /// Default Entity interfaces used for most of the case
    /// </summary>
    public interface IEntity: IEntity<string>
    {

    }
}
