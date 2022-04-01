namespace Silky.Http.Core;

public class WriteOptions
{
    public static readonly WriteOptions Default = new();

    private readonly WriteFlags flags;

    public WriteOptions(WriteFlags flags = (WriteFlags)0) => this.flags = flags;

    public WriteFlags Flags => this.flags;
}