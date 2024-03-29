﻿using System.Threading.Tasks;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IAsyncServerFilter : IServerFilterMetadata
{
    Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next);
}