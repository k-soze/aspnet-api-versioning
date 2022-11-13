// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class AmbiguousApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Ambiguous API Version";

    internal AmbiguousApiVersionEndpoint( ILogger logger )
        : base( c => OnExecute( c, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, ILogger logger )
    {
        var errorContextFactory = context.RequestServices.GetRequiredService<IApiVersionErrorContextFactory>();
        var errorContextWriter = context.RequestServices.GetRequiredService<IApiVersionErrorContextWriter>();
        var errorContext = errorContextFactory.CreateForAmbiguousApiVersion( context );
        if ( errorContextWriter.CanWrite( errorContext ) )
        {
            return errorContextWriter.WriteAsync( errorContext );
        }

        return Task.CompletedTask;
    }
}