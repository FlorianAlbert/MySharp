namespace FlorianAlbert.MySharp.Sdk.Parser.Test;

internal static class TheoryDataExtensions
{
    extension<T1, T2>(TheoryData<T1, T2> theoryData)
    {
        public IEnumerable<(T1, T2)> GetTuples()
        {
            foreach (object[]? data in theoryData)
            {
                yield return ((T1)data[0], (T2)data[1]);
            }
        }
    }
}
