﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_using_conventions;

using Asp.Versioning;
using Asp.Versioning.OData.UsingConventions;
using static System.Net.HttpStatusCode;

public class when_using_a_query_string_and_split_into_two_types : ConventionsAcceptanceTest
{
    [Theory]
    [InlineData( "api/people?api-version=1.0" )]
    [InlineData( "api/people/42?api-version=1.0" )]
    [InlineData( "api/people?api-version=2.0" )]
    [InlineData( "api/people/42?api-version=2.0" )]
    [InlineData( "api/people?api-version=3.0" )]
    [InlineData( "api/people/42?api-version=3.0" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange


        // act
        var response = ( await GetAsync( requestUrl ) ).EnsureSuccessStatusCode();

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
    }

    [Fact]
    public async Task then_get_should_return_404_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/people?api-version=4.0" );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    [Fact]
    public async Task then_patch_should_return_204()
    {
        // arrange
        var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

        // act
        var response = await PatchAsync( "api/people/42?api-version=2.0", person );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    [Theory]
    [InlineData( "api/people/42?api-version=1.0" )]
    [InlineData( "api/people/42?api-version=3.0" )]
    public async Task then_patch_should_return_400_if_supported_in_any_version( string requestUrl )
    {
        // arrange
        var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

        // act
        var response = await PatchAsync( requestUrl, person );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Fact]
    public async Task then_patch_should_return_404_for_an_unsupported_version()
    {
        // arrange
        var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

        // act
        var response = await PatchAsync( "api/people/42?api-version=4.0", person );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange

        // act
        var response = await GetAsync( "api/people" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert

        // change from 3.1 to 6.0; DELETE is version-neutral
        // and the only candidate, so GET returns 405
#if NETCOREAPP3_1
        response.StatusCode.Should().Be( MethodNotAllowed );
#else
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
#endif
    }

    public when_using_a_query_string_and_split_into_two_types( ConventionsFixture fixture ) : base( fixture ) { }
}