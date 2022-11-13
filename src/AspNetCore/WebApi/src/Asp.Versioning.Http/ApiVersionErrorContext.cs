// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Defines the context of a request with an API version error.
/// </summary>
/// <remarks>Used as intermediate object created when the API version error occured
/// and that will be used to write the error response to the client.</remarks>
public class ApiVersionErrorContext
{
    /// <summary>
    /// Gets the <see cref="HttpContent"/> associated with the current request being processed.
    /// </summary>
    [CLSCompliant( false )]
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Gets or sets the HTTP status code for the error response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets the HTTP status code for the error response.
    /// </summary>
    public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionErrorContext"/> class.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContent"/> associated with the current request being processed.</param>
    /// <param name="statusCode">The HTTP status code for the error response.</param>
    /// <param name="errorCode">The error code.</param>
    [CLSCompliant( false )]
    public ApiVersionErrorContext( HttpContext httpContext, int statusCode, string errorCode )
    {
        HttpContext = httpContext ?? throw new ArgumentNullException( nameof( httpContext ) );
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}