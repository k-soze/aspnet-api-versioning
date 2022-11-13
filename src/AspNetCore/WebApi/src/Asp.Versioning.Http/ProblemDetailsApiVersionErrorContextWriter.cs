// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Defines a type that writes <see cref="ProblemDetails"/> based on a <see cref="ApiVersionErrorContext"/>
/// payload to the current <see cref="HttpContext.Response"/>.
/// </summary>
public class ProblemDetailsApiVersionErrorContextWriter : IApiVersionErrorContextWriter
{
    private readonly IProblemDetailsFactory problemDetailsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsApiVersionErrorContextWriter"/> class.
    /// </summary>
    /// <param name="problemDetailsFactory">The <see cref="IProblemDetailsFactory"/>.</param>
    /// <exception cref="ArgumentNullException">problemDetailsFactory cannot be null.</exception>
    [CLSCompliant(false)]
    public ProblemDetailsApiVersionErrorContextWriter( IProblemDetailsFactory problemDetailsFactory)
    {
        this.problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException( nameof( problemDetailsFactory ) );
    }

    /// <inheritdoc/>
    public bool CanWrite( ApiVersionErrorContext context )
    {
        return true;
    }

    /// <inheritdoc/>
    public Task WriteAsync( ApiVersionErrorContext context )
    {
        if ( context is null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var httpContext = context.HttpContext;

        context.Extensions.TryGetValue( "type", out string? errorType );
        context.Extensions.TryGetValue( "instance", out string? errorInstance );

        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext.Request,
            context.StatusCode,
            context.Title,
            errorType,
            context.Message,
            errorInstance);

        httpContext.Response.StatusCode = context.StatusCode;
        return httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json);
    }
}