public interface IRng
{
    int Next();
    int Next(int minInclusive, int maxExclusive);
}
