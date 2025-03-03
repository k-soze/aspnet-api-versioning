﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
using System.Web.Http;
using System.Web.Http.Controllers;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query.Validator;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
#endif
using static Moq.Times;

public class ODataActionQueryOptionsConventionBuilderExtensionsTest
{
    [Fact]
    public void allow_orderby_should_enable_query_option_with_property_and_max_expressions()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilder();

        // act
        builder.AllowOrderBy( 42, "name" );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void allow_orderby_should_enable_query_option_with_property()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilder();

        // act
        builder.AllowOrderBy( "name" );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void allow_orderby_should_enable_query_option_with_properties()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilder();
        var properties = new[] { "name" }.AsEnumerable();

        // act
        builder.AllowOrderBy( properties );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void allow_orderby_of_t_should_enable_query_option_with_property_and_max_expressions()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilderOfT();

        // act
        builder.AllowOrderBy( 42, "name" );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void allow_orderby_of_t_should_enable_query_option_with_property()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilderOfT();

        // act
        builder.AllowOrderBy( "name" );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void allow_orderby_of_t_should_enable_query_option_with_properties()
    {
        // arrange
        var builder = new TestODataActionQueryOptionsConventionBuilderOfT();
        var properties = new[] { "name" }.AsEnumerable();

        // act
        builder.AllowOrderBy( properties );

        // assert
        builder.ValidationSettings.Should().BeEquivalentTo(
            new
            {
                AllowedQueryOptions = OrderBy,
                AllowedOrderByProperties = new[] { "name" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void action_should_map_method_from_action_delegate_expression()
    {
        // arrange
        var method = typeof( StubController ).GetMethod( nameof( StubController.Delete ) );
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder<StubController>();
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder<StubController>>( controllerBuilder );

        // act
        builder.Object.Action( c => c.Delete() );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_map_method_from_func_delegate_expression()
    {
        // arrange
        var method = typeof( StubController ).GetMethod( nameof( StubController.Get ) );
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder<StubController>();
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder<StubController>>( controllerBuilder );

        // act
        builder.Object.Action( c => c.Get() );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_throw_exception_when_func_delegate_expression_is_not_a_method()
    {
        // arrange
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder<StubController>();
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder<StubController>>( controllerBuilder ).Object;

        // act
        Action action = () => builder.Action( c => c.Timeout );

        // assert
        action.Should().Throw<InvalidOperationException>().And
              .Message.Should().Be( "The expression 'c => c.Timeout' must refer to a controller action method." );
    }

    [Fact]
    public void action_should_map_method_from_name()
    {
        // arrange
        const string methodName = nameof( StubController.Post );
        var controllerType = typeof( StubController );
        var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 0 );
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder( controllerType );
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder>( controllerBuilder ) { CallBase = true };

        // act
        builder.Object.Action( methodName );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_map_method_from_name_and_argument_type()
    {
        // arrange
        const string methodName = nameof( StubController.Post );
        var controllerType = typeof( StubController );
        var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 1 );
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder( controllerType );
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder>( controllerBuilder ) { CallBase = true };

        // act
        builder.Object.Action( methodName, typeof( int ) );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_throw_exception_when_method_does_not_exist()
    {
        // arrange
        var message = "An action method with the name 'NoSuchMethod' could not be found. The method must be public, non-static, and not have the NonActionAttribute applied.";
        var controllerBuilder = new ODataControllerQueryOptionsConventionBuilder( typeof( StubController ) );
        var builder = new Mock<ODataActionQueryOptionsConventionBuilder>( controllerBuilder ) { CallBase = true };

        // act
        Action actionConvention = () => builder.Object.Action( "NoSuchMethod" );

        // assert
        actionConvention.Should().Throw<MissingMethodException>().And.Message.Should().Be( message );
    }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1822 // Mark members as static

    public sealed class StubController : ControllerBase
    {
        public IActionResult Get() => Ok();

        public void Delete() { }

        public TimeSpan Timeout { get; set; }

        public IActionResult Post() => Post( 42, "stubs/42" );

        public IActionResult Post( int id ) => Ok();

        [NonAction]
        public IActionResult Post( int id, string location ) => Created( location, new { id } );
    }

    private sealed class TestODataActionQueryOptionsConventionBuilder : ODataActionQueryOptionsConventionBuilder
    {
        internal TestODataActionQueryOptionsConventionBuilder()
            : base( new ODataControllerQueryOptionsConventionBuilder( typeof( ControllerBase ) ) ) { }

        internal new ODataValidationSettings ValidationSettings => base.ValidationSettings;
    }

    private sealed class TestODataActionQueryOptionsConventionBuilderOfT : ODataActionQueryOptionsConventionBuilder<ControllerBase>
    {
        internal TestODataActionQueryOptionsConventionBuilderOfT()
            : base( new ODataControllerQueryOptionsConventionBuilder<ControllerBase>() ) { }

        internal new ODataValidationSettings ValidationSettings => base.ValidationSettings;
    }
}