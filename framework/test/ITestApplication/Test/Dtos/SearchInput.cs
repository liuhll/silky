namespace ITestApplication.Test.Dtos;

public class SearchInput
{
    public string Name { get; set; }
    
    public string Address { get; set; }

    public int PageSize { get; set; } = 10;

    public int PageIndex { get; set; } = 1;
}