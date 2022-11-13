// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Represents a context creator for API version errors.
/// </summary>
[CLSCompliant( false )]
public interface IApiVersionErrorContextFactory
{
    /// <summary>
    /// Create a context for an ambiguous API version error.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> related to the error.</param>
    /// <returns>An instance of <see cref="ApiVersionErrorContext"/>.</returns>
    ApiVersionErrorContext CreateForAmbiguousApiVersion(HttpContext httpContext);

    /// <summary>
    /// Create a context for an malformed API version error.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> related to the error.</param>
    /// <returns>An instance of <see cref="ApiVersionErrorContext"/>.</returns>
    ApiVersionErrorContext CreateForMalformedApiVersion( HttpContext httpContext );

    /// <summary>
    /// Create a context for an unspecified API version error.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> related to the error.</param>
    /// <returns>An instance of <see cref="ApiVersionErrorContext"/>.</returns>
    ApiVersionErrorContext CreateForUnspecifiedApiVersion( HttpContext httpContext );

    /// <summary>
    /// Create a context for an unsupported API version error.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> related to the error.</param>
    /// <param name="statusCode">The HTTP status code of the error response.</param>
    /// <returns>An instance of <see cref="ApiVersionErrorContext"/>.</returns>
    ApiVersionErrorContext CreateForUnsupportedApiVersion( HttpContext httpContext, int statusCode );
}