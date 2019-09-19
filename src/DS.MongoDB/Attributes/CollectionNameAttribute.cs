using System;

namespace CWT.Infrastructure.Repository.Mongo.Attributes
{
    /// <summary>
    /// This attribute allows you to specify of the name of the collection.
    /// who has included the CollectionName attribute into the repo to give another choice to the user on how 
    /// to name their collections. 
    /// The attribute takes precedence of course, and if not present the library will fall back to your Pluralize method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute : Attribute
    {
        /// <summary>
        /// The name of the collection in which your documents are stored.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="name">The name of the collection.</param>
        public CollectionNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}