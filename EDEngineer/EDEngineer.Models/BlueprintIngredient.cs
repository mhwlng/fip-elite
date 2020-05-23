using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class BlueprintIngredient
    {
        public EntryData EntryData { get; }

        public BlueprintIngredient(EntryData entryData, int size)
        {
            EntryData = entryData;
            Size = size;
        }

        public int Size { get; }

        protected bool Equals(BlueprintIngredient other)
        {
            return Equals(EntryData, other.EntryData) && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BlueprintIngredient)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EntryData != null ? EntryData.GetHashCode() : 0) * 397) ^ Size;
            }
        }
    }
}