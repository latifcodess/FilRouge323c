namespace DataSeries;

public class DataSeries<T>
{
    private readonly IEnumerable<DataPoint<T>> _data;
    private DataSeries(IEnumerable<DataPoint<T>> data) => _data = data;
    public static DataSeries<T> From(IEnumerable<DataPoint<T>> source) => new DataSeries<T>(source);
    public int Count => _data.Count();
    public IEnumerable<T> Values => _data.Select(dp => dp.Value);
    public IEnumerable<DataPoint<T>> DataPoints => _data;

    
    public static DataSeries<T> FromCsv(string path, Func<string[], T> parser)
    {
        var lines = File.ReadAllLines(path).Skip(1); // ignorer l'en-tête
        return new DataSeries<T>(lines.Select(line => {
            var cols = line.Split(',');
            return new DataPoint<T>(DateTime.Parse(cols[0]), parser(cols));
        }));
    }
    public DataSeries<T> Filter(Func<T, bool> predicate)
        => new DataSeries<T>(_data.Where(dp => predicate(dp.Value)));
    
    public DataSeries<T> FilterByDate(Func<DateTime, bool> predicate)
        => new DataSeries<T>(_data.Where(dp => predicate(dp.Timestamp)));
    
    public DataSeries<T> RemoveOutliers(Func<T, bool> isValid)
        => Filter(isValid);
    
    public bool HasAny(Func<T, bool> predicate)
        => Values.Any(predicate);

    public bool AllMatch(Func<T, bool> predicate)
        => Values.All(predicate);
    
    public DataSeries<TResult> Transform<TResult>(Func<T, TResult> mapper)
        => DataSeries<TResult>.From(_data.Select(dp => new DataPoint<TResult>(dp.Timestamp, mapper(dp.Value))));
}