﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
#endif

public partial class ControllerApiVersionConventionBuilderTest
{
    [Fact]
    public void version_neutral_should_be_false_by_default()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        var versionNeutral = controllerBuilder.ProtectedVersionNeutral;

        // assert
        versionNeutral.Should().BeFalse();
    }

    [Fact]
    public void is_api_version_neutral_should_update_backing_property()
    {
        // arrange
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        controllerBuilder.IsApiVersionNeutral();

        // assert
        controllerBuilder.ProtectedVersionNeutral.Should().BeTrue();
    }

    [Fact]
    public void action_should_add_new_action_convention_builder()
    {
        // arrange
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();

        // act
        var actionBuilder = controllerBuilder.Action( method );

        // assert
        controllerBuilder.ProtectedActionBuilders.Single().Should().BeSameAs( actionBuilder );
    }

    [Fact]
    public void action_should_return_existing_action_convention_builder()
    {
        // arrange
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );
        var controllerBuilder = new TestControllerApiVersionConventionBuilder();
        var originalActionBuilder = controllerBuilder.Action( method );

        // act
        var actionBuilder = controllerBuilder.Action( method );

        // assert
        actionBuilder.Should().BeSameAs( originalActionBuilder );
        controllerBuilder.ProtectedActionBuilders.Single().Should().BeSameAs( actionBuilder );
    }

    private sealed class TestControllerApiVersionConventionBuilder : ControllerApiVersionConventionBuilder
    {
        internal TestControllerApiVersionConventionBuilder() : base( typeof( ControllerBase ) ) { }

        internal TestControllerApiVersionConventionBuilder( Type controllerType ) : base( controllerType ) { }

        internal bool ProtectedVersionNeutral => VersionNeutral;

        internal ActionApiVersionConventionBuilderCollection ProtectedActionBuilders => ActionBuilders;
    }

#pragma warning disable CA1812

#if !NETFRAMEWORK
    [ApiController]
#endif
    private sealed class UndecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();
    }

#if !NETFRAMEWORK
    [ApiController]
#endif
    [ApiVersion( "2.0" )]
    [ApiVersion( "0.9", Deprecated = true )]
    [AdvertiseApiVersions( "3.0" )]
    [AdvertiseApiVersions( "3.0-Beta", Deprecated = true )]
    private sealed class DecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();
    }
}