// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class ProblemDetailsApiVersionErrorContextWriterTest
{
    [Fact]
    public void create_instance_with_problem_details_factory_null_should_throw_exception()
    {
        // arrange
        IProblemDetailsFactory problemDetailsFactory = null;

        // act
        var act = () =>
        {
            _ = new ProblemDetailsApiVersionErrorContextWriter( problemDetailsFactory );
        };

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void can_write_should_return_true()
    {
        // arrange
        Mock<IProblemDetailsFactory> problemDetailsFactoryMock = new();
        var target = new ProblemDetailsApiVersionErrorContextWriter( problemDetailsFactoryMock.Object );

        // act
        var actual = target.CanWrite( null );

        // assert
        actual.Should().BeTrue();
    }

    [Fact]
    public async Task write_async_with_context_null_should_throw_exception()
    {
        // arrange
        Mock<IProblemDetailsFactory> problemDetailsFactoryMock = new();
        var target = new ProblemDetailsApiVersionErrorContextWriter( problemDetailsFactoryMock.Object );

        // act
        var act = async () =>
        {
            await target.WriteAsync( null );
        };

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task write_async_should_create_problem_details_using_factory()
    {
        // arrange
        Mock<IProblemDetailsFactory> problemDetailsFactoryMock = new();
        var target = new ProblemDetailsApiVersionErrorContextWriter( problemDetailsFactoryMock.Object );
        Mock<HttpRequest> requestMock = new();
        Mock<HttpResponse> responseMock = new();
        MemoryStream responseBody = new();
        responseMock.SetupGet( x => x.Body ).Returns( responseBody );
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.SetupGet( x => x.Request ).Returns( requestMock.Object );
        httpContextMock.SetupGet( x => x.Response ).Returns( responseMock.Object );
        responseMock.SetupGet( x => x.HttpContext ).Returns( httpContextMock.Object );
        ApiVersionErrorContext errorContext = new( httpContextMock.Object, 400, "TestError" )
        {
            Message = "TestMessage",
            Title = "TestTitle",
        };
        errorContext.Extensions.Add( "type", "https://api.versioning/error/type" );
        errorContext.Extensions.Add( "instance", "ErrorInstance" );

        // act
        await target.WriteAsync( errorContext );

        // assert
        problemDetailsFactoryMock.Verify(
            x => x.CreateProblemDetails(
                It.IsAny<HttpRequest>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>() ),
            Times.Once );
        problemDetailsFactoryMock.Verify(
            x => x.CreateProblemDetails(
                errorContext.HttpContext.Request,
                400,
                "TestTitle",
                "https://api.versioning/error/type",
                "TestMessage",
                "ErrorInstance" ),
            Times.Once );

        // clean up
        await responseBody.DisposeAsync();
    }

    [Fact]
    public async Task write_async_should_set_status_code_and_write_creates_problem_details_to_response()
    {
        // arrange
        Mock<IProblemDetailsFactory> problemDetailsFactoryMock = new();
        ProblemDetails problemDetails = new()
        {
            Title = "TestTitle",
        };
        problemDetailsFactoryMock.Setup( x =>
            x.CreateProblemDetails(
                It.IsAny<HttpRequest>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>() ) )
            .Returns( problemDetails );
        var target = new ProblemDetailsApiVersionErrorContextWriter( problemDetailsFactoryMock.Object );
        Mock<HttpRequest> requestMock = new();
        Mock<HttpResponse> responseMock = new();
        MemoryStream responseBody = new();
        responseMock.SetupGet( x => x.Body ).Returns( responseBody );
        var responseStatusCode = 0;
        responseMock.SetupSet( x => x.StatusCode = It.IsAny<int>() )
            .Callback<int>( sc => responseStatusCode = sc );
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.SetupGet( x => x.Request ).Returns( requestMock.Object );
        httpContextMock.SetupGet( x => x.Response ).Returns( responseMock.Object );
        responseMock.SetupGet( x => x.HttpContext ).Returns( httpContextMock.Object );
        ApiVersionErrorContext errorContext = new( httpContextMock.Object, 400, "TestError" )
        {
            Message = "TestMessage",
            Title = "TestTitle",
        };
        errorContext.Extensions.Add( "type", "https://api.versioning/error/type" );
        errorContext.Extensions.Add( "instance", "ErrorInstance" );

        // act
        await target.WriteAsync( errorContext );

        // assert
        responseStatusCode.Should().Be( 400 );
        responseBody.Position = 0;
        var problemDetailsFromResponse =
            await JsonSerializer.DeserializeAsync<ProblemDetails>(
                responseBody,
                options: default);
        problemDetailsFromResponse.Title.Should().Be( "TestTitle" );

        // clean up
        await responseBody.DisposeAsync();
    }
}