using System;

namespace AppCore.Data
{
    public readonly struct VersionId : IEquatable<VersionId>
    {
        public int Id { get; }

        public int Version { get; }

        public VersionId(int id, int version)
        {
            Id = id;
            Version = version;
        }

        public bool Equals(VersionId other)
        {
            return Id == other.Id && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is VersionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id + Version;
        }

        public static bool operator ==(VersionId left, VersionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VersionId left, VersionId right)
        {
            return !left.Equals(right);
        }
    }
}