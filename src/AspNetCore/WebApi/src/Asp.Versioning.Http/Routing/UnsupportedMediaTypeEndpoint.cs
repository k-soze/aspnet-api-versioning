// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedMediaTypeEndpoint : Endpoint
{
    private const string Name = "415 HTTP Unsupported Media Type";

    internal UnsupportedMediaTypeEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context )
    {
        var errorContextFactory = context.RequestServices.GetRequiredService<IApiVersionErrorContextFactory>();
        var errorContextWriter = context.RequestServices.GetRequiredService<IApiVersionErrorContextWriter>();
        var errorContext = errorContextFactory.CreateForUnsupportedApiVersion( context, StatusCodes.Status415UnsupportedMediaType );
        if ( errorContextWriter.CanWrite( errorContext ) )
        {
            return errorContextWriter.WriteAsync( errorContext );
        }

        return Task.CompletedTask;
    }
}