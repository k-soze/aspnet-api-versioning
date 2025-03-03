﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ApiController_using_conventions;

using Asp.Versioning;
using Asp.Versioning.Http.UsingConventions;
using Asp.Versioning.Http.UsingConventions.Controllers;
using System.Net.Http;
using static System.Net.HttpStatusCode;

[Collection( nameof( ConventionsTestCollection ) )]
public class when_using_a_query_string_and_split_into_two_types : AcceptanceTest
{
    [Theory]
    [InlineData( nameof( ValuesController ), "1.0" )]
    [InlineData( nameof( Values2Controller ), "2.0" )]
    [InlineData( nameof( Values2Controller ), "3.0" )]
    public async Task then_get_should_return_200( string controller, string apiVersion )
    {
        // arrange


        // act
        var response = await GetAsync( $"api/values?api-version={apiVersion}" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsAsync<IDictionary<string, string>>();

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        content.Should().BeEquivalentTo(
            new Dictionary<string, string>()
            {
                ["controller"] = controller,
                ["version"] = apiVersion,
            } );
    }

    [Fact]
    public async Task then_get_should_return_404_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=4.0" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( NotFound );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
    }

    public when_using_a_query_string_and_split_into_two_types( ConventionsFixture fixture ) : base( fixture ) { }
}