// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

/// <summary>
/// Defines a type that writes an error based on a <see cref="ApiVersionErrorContext"/>
/// payload to the current <see cref="HttpContext.Response"/>.
/// </summary>
public interface IApiVersionErrorContextWriter
{
    /// <summary>
    /// Writes the error response according to the specified context.
    /// </summary>
    /// <param name="context">The <see cref="ApiVersionErrorContext"/> associated with the current request/response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteAsync( ApiVersionErrorContext context );

    /// <summary>
    /// Determines whether this instance can write an error to the specified context.
    /// </summary>
    /// <param name="context">The <see cref="ApiVersionErrorContext"/> associated with the current request/response.</param>
    /// <returns>Flag that indicates if that the writer can write to the specified <see cref="ApiVersionErrorContext"/>.</returns>
    bool CanWrite( ApiVersionErrorContext context );
}