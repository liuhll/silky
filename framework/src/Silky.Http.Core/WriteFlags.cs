using System;

namespace Silky.Http.Core;

[Flags]
public enum WriteFlags
{

    BufferHint = 1,

    NoCompress = 2,
}