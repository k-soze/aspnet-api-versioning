// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;

/// <summary>
/// Defines the default implementation for the creating context for API version errors.
/// </summary>
public class ApiVersionErrorContextFactory : IApiVersionErrorContextFactory
{
    /// <inheritdoc/>
    [CLSCompliant( false )]
    public ApiVersionErrorContext CreateForAmbiguousApiVersion( HttpContext httpContext )
    {
        if ( httpContext is null )
        {
            throw new ArgumentNullException( nameof( httpContext ) );
        }

        var apiVersions = httpContext.ApiVersioningFeature().RawRequestedApiVersions;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            CommonSR.MultipleDifferentApiVersionsRequested,
            string.Join( ", ", apiVersions ) );
        var errorContext = CreateContext(
            httpContext,
            StatusCodes.Status400BadRequest,
            ProblemDetailsDefaults.Ambiguous,
            detail );
        return errorContext;
    }

    /// <inheritdoc/>
    [CLSCompliant( false )]
    public ApiVersionErrorContext CreateForMalformedApiVersion( HttpContext httpContext )
    {
        if ( httpContext is null )
        {
            throw new ArgumentNullException( nameof( httpContext ) );
        }

        var requestUrl = new Uri( httpContext.Request.GetDisplayUrl() ).SafeFullPath();
        var requestedVersion = httpContext.ApiVersioningFeature().RawRequestedApiVersion;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            SR.VersionedResourceNotSupported,
            requestUrl,
            requestedVersion );
        var errorContext = CreateContext(
            httpContext,
            StatusCodes.Status400BadRequest,
            ProblemDetailsDefaults.Invalid,
            detail );
        return errorContext;
    }

    /// <inheritdoc/>
    [CLSCompliant( false )]
    public ApiVersionErrorContext CreateForUnspecifiedApiVersion( HttpContext httpContext )
    {
        if ( httpContext is null )
        {
            throw new ArgumentNullException( nameof( httpContext ) );
        }

        var detail = SR.ApiVersionUnspecified;
        var errorContext = CreateContext(
            httpContext,
            StatusCodes.Status400BadRequest,
            ProblemDetailsDefaults.Unspecified,
            detail );
        return errorContext;
    }

    /// <inheritdoc/>
    [CLSCompliant( false )]
    public ApiVersionErrorContext CreateForUnsupportedApiVersion( HttpContext httpContext, int statusCode )
    {
        if ( httpContext is null )
        {
            throw new ArgumentNullException( nameof( httpContext ) );
        }

        var url = new Uri( httpContext.Request.GetDisplayUrl() ).SafeFullPath();
        var apiVersion = httpContext.ApiVersioningFeature().RawRequestedApiVersion;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            SR.VersionedResourceNotSupported,
            url,
            apiVersion );
        var errorContext = CreateContext(
            httpContext,
            statusCode,
            ProblemDetailsDefaults.Unsupported,
            detail );
        return errorContext;
    }

    private static ApiVersionErrorContext CreateContext(
        HttpContext httpContext,
        int statusCode,
        ProblemDetailsInfo problemDetailsInfo,
        string detail)
    {
        var errorContext = new ApiVersionErrorContext(
            httpContext,
            statusCode,
            problemDetailsInfo.Code ?? "UnexpectedError" )
        {
            Title = problemDetailsInfo.Title,
            Message = detail,
        };
        errorContext.Extensions.Add( "type", problemDetailsInfo.Type );

        return errorContext;
    }
}