﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using System.Reflection;
using static Moq.Times;

public partial class ActionApiVersionConventionBuilderTTest
{
    [Fact]
    public void action_should_call_action_on_controller_builder()
    {
        // arrange
        var controllerBuilder = new Mock<ControllerApiVersionConventionBuilder<UndecoratedController>>();
        var actionBuilder = new ActionApiVersionConventionBuilder<UndecoratedController>( controllerBuilder.Object );
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );

        controllerBuilder.Setup( cb => cb.Action( It.IsAny<MethodInfo>() ) );

        // act
        actionBuilder.Action( method );

        // assert
        controllerBuilder.Verify( cb => cb.Action( method ), Once() );
    }

#pragma warning disable CA1034 // Nested types should not be visible
#if !NETFRAMEWORK
    [ApiController]
#endif
    public sealed class UndecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();
    }

#if !NETFRAMEWORK
    [ApiController]
#endif
    public sealed class DecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();

        [MapToApiVersion( "2.0" )]
        [MapToApiVersion( "3.0" )]
        public IActionResult GetV2() => Ok();
    }
}