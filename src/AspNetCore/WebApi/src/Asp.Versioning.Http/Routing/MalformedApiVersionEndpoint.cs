// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class MalformedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Invalid API Version";

    internal MalformedApiVersionEndpoint( ILogger logger )
        : base( c => OnExecute( c, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, ILogger logger )
    {
        var requestedVersion = context.ApiVersioningFeature().RawRequestedApiVersion;

        var errorContextFactory = context.RequestServices.GetRequiredService<IApiVersionErrorContextFactory>();
        var errorContextWriter = context.RequestServices.GetRequiredService<IApiVersionErrorContextWriter>();
        var errorContext = errorContextFactory.CreateForMalformedApiVersion( context );
        logger.ApiVersionInvalid( requestedVersion );
        if ( errorContextWriter.CanWrite( errorContext ) )
        {
            return errorContextWriter.WriteAsync( errorContext );
        }

        return Task.CompletedTask;
    }
}