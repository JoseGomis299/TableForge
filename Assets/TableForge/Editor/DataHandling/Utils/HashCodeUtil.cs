namespace TableForge.Editor
{
    public static class HashCodeUtil
    {
        public static int CombineHashes(params object[] values)
        {
            unchecked // Allow arithmetic overflow without exceptions
            {
                int hash = 17;
                foreach (var value in values)
                {
                    int valueHash = value != null ? GetDeterministicHashCode(value) : 0;
                    hash = hash * 23 + valueHash;
                }
                return hash;
            }
        }

        private static int GetDeterministicHashCode(object obj)
        {
            if (obj == null)
                return 0;

            if (obj is string str)
                return GetDeterministicStringHashCode(str);
            
            return obj.GetHashCode(); // Fallback to default (note: may be non-deterministic)
        }

        private static int GetDeterministicStringHashCode(string str)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in str)
                {
                    hash = (hash * 31) + c;
                }
                return hash;
            }
        }

    }
}