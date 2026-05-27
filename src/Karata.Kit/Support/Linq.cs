namespace Karata.Kit.Support;

public static class Linq
{
    extension<T>(IEnumerable<T> list)
    {
        public IEnumerable<IEnumerable<T>> Permutations()
        {
            var items = list.ToList();
            if (items.Count <= 1)
                return new[] { items };

            return items.SelectMany((item, index) =>
                    items.Where((_, i) => i != index) // Get all items except the current one
                        .Permutations() // Recursively get their permutations
                        .Select(p => new[] { item }.Concat(p)) // Prepend current item
            );
        }
    }
}