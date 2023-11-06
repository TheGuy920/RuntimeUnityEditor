using Plasma.Mods.RuntimeUnityEditor.Core.Utils;

namespace Plasma.Mods.RuntimeUnityEditor.Core.Inspector.Entries
{
    public class ReadonlyListCacheEntry : ReadonlyCacheEntry
    {
        public ReadonlyListCacheEntry(object o, int index) : base(GetListItemName(index), o)
        {
        }

        internal static string GetListItemName(int index)
        {
            return "Index: " + index;
        }

        public override string ToString()
        {
            var isNull = Object.IsNullOrDestroyed();
            if (isNull != null) return "[" + isNull + "]";

            return Object.ToString();
        }
    }
}
