// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unsupported API Version";

    internal UnsupportedApiVersionEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context )
    {
        var errorContextFactory = context.RequestServices.GetRequiredService<IApiVersionErrorContextFactory>();
        var errorContextWriter = context.RequestServices.GetRequiredService<IApiVersionErrorContextWriter>();
        var errorContext = errorContextFactory.CreateForUnsupportedApiVersion( context, StatusCodes.Status400BadRequest );
        if ( errorContextWriter.CanWrite( errorContext ) )
        {
            return errorContextWriter.WriteAsync( errorContext );
        }

        return Task.CompletedTask;
    }
}