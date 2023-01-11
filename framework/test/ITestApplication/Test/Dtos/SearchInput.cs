namespace ITestApplication.Test.Dtos;

public class SearchInput
{
    public string Name { get; set; }
    
    public string Address { get; set; }

    public long[] Ids { get; set; }

    public int PageSize { get; set; } = 10;

    public int PageIndex { get; set; } = 1;

    public Sort[] Sorts { get; set; }
}

public class Sort
{
    public string Filed { get; set; }

    public int OrderBy { get; set; }
}