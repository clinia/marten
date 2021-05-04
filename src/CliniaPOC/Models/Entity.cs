using System;
using System.Collections.Generic;

namespace CliniaPOC.Models
{
    public abstract class Entity
    {
        public Entity()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Entity(string type) : this()
        {
            Type = type;
        }

        public string Id { get; set; }

        public string Type { get; }

        public Dictionary<string, object> Values { get; set; }
    }
}
