// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnspecifiedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unspecified API Version";

    internal UnspecifiedApiVersionEndpoint( ILogger logger, string[]? displayNames = default )
        : base( c => OnExecute( c, displayNames, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, string[]? candidateEndpoints, ILogger logger )
    {
        var errorContextFactory = context.RequestServices.GetRequiredService<IApiVersionErrorContextFactory>();
        var errorContextWriter = context.RequestServices.GetRequiredService<IApiVersionErrorContextWriter>();
        var errorContext = errorContextFactory.CreateForMalformedApiVersion( context );

        if ( candidateEndpoints == null || candidateEndpoints.Length == 0 )
        {
            logger.ApiVersionUnspecified();
        }
        else
        {
            logger.ApiVersionUnspecifiedWithCandidates( candidateEndpoints );
        }

        if ( errorContextWriter.CanWrite( errorContext ) )
        {
            return errorContextWriter.WriteAsync( errorContext );
        }

        return Task.CompletedTask;
    }
}