namespace ITestApplication.Test.Dtos;

public class ResponseBase
{
    public ResponseBase(int code = 520, string message = "未知异常")
    {
        Code = code;

        Message = message;
    }

    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 返回描述
    /// </summary>
    public string Message { get; set; }
}