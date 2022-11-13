// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Globalization;

public class ApiVersionErrorContextFactoryTest
{
    [Fact]
    public void create_for_ambiguous_api_version_with_http_context_null_should_throw_exception()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();

        // act
        var act = () => _ = target.CreateForAmbiguousApiVersion( null );

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void create_for_ambiguous_api_version_should_return_error_context()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();
        Mock<IApiVersioningFeature> apiVersioningFeaturesMock = new();
        Mock<IFeatureCollection> featuresMock = new();
        featuresMock.Setup( x => x.Get<IApiVersioningFeature>() )
            .Returns( apiVersioningFeaturesMock.Object );
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup( x => x.Features )
            .Returns( featuresMock.Object );
        var apiVersions = new List<string>() { "1.0", "1.1" };

        apiVersioningFeaturesMock.SetupGet( x => x.RawRequestedApiVersions )
            .Returns( apiVersions );
        var problemDetailsInfo = ProblemDetailsDefaults.Ambiguous;
        var multipleDifferentApiVersionsRequested = "The following API versions were requested: {0}. At most, only a single API version may be specified. Please update the intended API version and retry the request.";

        // act
        var actual = target.CreateForAmbiguousApiVersion( httpContextMock.Object );

        // assert
        ErrorContextShouldSatisfy(
            actual,
            httpContextMock.Object,
            StatusCodes.Status400BadRequest,
            problemDetailsInfo,
            string.Format(
                CultureInfo.CurrentCulture,
                multipleDifferentApiVersionsRequested,
                string.Join( ", ", apiVersions ) ) );
    }

    [Fact]
    public void create_for_malformed_api_version_with_http_context_null_should_throw_exception()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();

        // act
        var act = () => _ = target.CreateForMalformedApiVersion( null );

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void create_for_malformed_api_version_should_return_error_context()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();
        Mock<IApiVersioningFeature> apiVersioningFeaturesMock = new();
        Mock<IFeatureCollection> featuresMock = new();
        featuresMock.Setup( x => x.Get<IApiVersioningFeature>() )
            .Returns( apiVersioningFeaturesMock.Object );
        Mock<HttpRequest> requestMock = new();
        requestMock.SetupGet( x => x.Scheme ).Returns( "https" );
        requestMock.SetupGet( x => x.Host ).Returns( new HostString( "api.versioning" ) );
        requestMock.SetupGet( x => x.PathBase ).Returns( new PathString( "/test" ) );
        requestMock.SetupGet( x => x.Path ).Returns( new PathString( "/url" ) );
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup( x => x.Features )
            .Returns( featuresMock.Object );
        httpContextMock.SetupGet( x => x.Request ).Returns( requestMock.Object );

        apiVersioningFeaturesMock.SetupGet( x => x.RawRequestedApiVersion )
            .Returns( "1.1" );
        var problemDetailsInfo = ProblemDetailsDefaults.Invalid;
        var versionedResourceNotSupported = "The HTTP resource that matches the request URI '{0}' does not support the API version '{1}'.";

        // act
        var actual = target.CreateForMalformedApiVersion( httpContextMock.Object );

        // assert
        ErrorContextShouldSatisfy(
            actual,
            httpContextMock.Object,
            StatusCodes.Status400BadRequest,
            problemDetailsInfo,
            string.Format(
                CultureInfo.CurrentCulture,
                versionedResourceNotSupported,
                "https://api.versioning/test/url",
                "1.1" ) );
    }

    [Fact]
    public void create_for_unspecified_api_version_with_http_context_null_should_throw_exception()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();

        // act
        var act = () => _ = target.CreateForUnspecifiedApiVersion( null );

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void create_for_unspecified_api_version_should_return_error_context()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();
        Mock<HttpContext> httpContextMock = new();

        var problemDetailsInfo = ProblemDetailsDefaults.Unspecified;
        var apiVersionUnspecified = "An API version is required, but was not specified.";

        // act
        var actual = target.CreateForUnspecifiedApiVersion( httpContextMock.Object );

        // assert
        ErrorContextShouldSatisfy(
            actual,
            httpContextMock.Object,
            StatusCodes.Status400BadRequest,
            problemDetailsInfo,
            apiVersionUnspecified );
    }

    [Fact]
    public void create_for_unsupported_api_version_with_http_context_null_should_throw_exception()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();

        // act
        var act = () => _ = target.CreateForUnsupportedApiVersion( null, 400 );

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void create_for_unsupported_api_version_should_return_error_context()
    {
        // arrange
        var target = new ApiVersionErrorContextFactory();
        Mock<IApiVersioningFeature> apiVersioningFeaturesMock = new();
        Mock<IFeatureCollection> featuresMock = new();
        featuresMock.Setup( x => x.Get<IApiVersioningFeature>() )
            .Returns( apiVersioningFeaturesMock.Object );
        Mock<HttpRequest> requestMock = new();
        requestMock.SetupGet( x => x.Scheme ).Returns( "https" );
        requestMock.SetupGet( x => x.Host ).Returns( new HostString( "api.versioning" ) );
        requestMock.SetupGet( x => x.PathBase ).Returns( new PathString( "/test" ) );
        requestMock.SetupGet( x => x.Path ).Returns( new PathString( "/url" ) );
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup( x => x.Features )
            .Returns( featuresMock.Object );
        httpContextMock.SetupGet( x => x.Request ).Returns( requestMock.Object );

        apiVersioningFeaturesMock.SetupGet( x => x.RawRequestedApiVersion )
            .Returns( "1.1" );
        var problemDetailsInfo = ProblemDetailsDefaults.Unsupported;
        var versionedResourceNotSupported = "The HTTP resource that matches the request URI '{0}' does not support the API version '{1}'.";

        // act
        var actual = target.CreateForUnsupportedApiVersion( httpContextMock.Object, StatusCodes.Status409Conflict );

        // assert
        ErrorContextShouldSatisfy(
            actual,
            httpContextMock.Object,
            StatusCodes.Status409Conflict,
            problemDetailsInfo,
            string.Format(
                CultureInfo.CurrentCulture,
                versionedResourceNotSupported,
                "https://api.versioning/test/url",
                "1.1" ) );
    }

    private static void ErrorContextShouldSatisfy(
        ApiVersionErrorContext actual,
        HttpContext httpContext,
        int statusCode,
        ProblemDetailsInfo problemDetailsInfo,
        string detail,
        string instance = null)
    {
        actual.HttpContext.Should().BeSameAs( httpContext );
        actual.StatusCode.Should().Be( statusCode );
        actual.ErrorCode.Should().Be( problemDetailsInfo.Code );
        actual.Title.Should().Be( problemDetailsInfo.Title );
        actual.Message.Should().Be( detail );
        actual.Extensions.Should().HaveCount( instance is null ? 1 : 2 );
        actual.Extensions.Should().Contain( "type", problemDetailsInfo.Type );
        if (instance is not null)
        {
            actual.Extensions.Should().Contain( "instance", instance );
        }
    }
}