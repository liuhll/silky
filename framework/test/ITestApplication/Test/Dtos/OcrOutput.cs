namespace ITestApplication.Test.Dtos;

public class OcrOutput : ResponseBase
{
    /// <summary>
    /// 识别结果
    /// </summary>
    public object Result { get; set; }
}