﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

public class VersionedApiDescriptionProviderTest
{
    [Fact]
    public void versioned_api_explorer_should_group_and_order_descriptions_on_providers_executed()
    {
        // arrange
        var actionProvider = new TestActionDescriptorCollectionProvider();
        var context = new ApiDescriptionProviderContext( actionProvider.ActionDescriptors.Items );
        var apiExplorer = new VersionedApiDescriptionProvider(
            Mock.Of<ISunsetPolicyManager>(),
            NewModelMetadataProvider(),
            Options.Create( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } ) );

        for ( var i = 0; i < context.Actions.Count; i++ )
        {
            context.Results.Add( new() { ActionDescriptor = context.Actions[i] } );
        }

        // act
        apiExplorer.OnProvidersExecuted( context );

        // assert
        context.Results.Should().BeEquivalentTo(
            new[]
            {
                // orders
                new { GroupName = "v0.9", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 0, 9 ) } },
                new { GroupName = "v1", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                new { GroupName = "v1", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },

                // people
                new { GroupName = "v0.9", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 0, 9 ) } },
                new { GroupName = "v1", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                new { GroupName = "v1", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v2", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                new { GroupName = "v3", Properties = new Dictionary<object, object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void versioned_api_explorer_should_apply_sunset_policy()
    {
        // arrange
        var actionProvider = new TestActionDescriptorCollectionProvider();
        var context = new ApiDescriptionProviderContext( actionProvider.ActionDescriptors.Items );
        var expected = new SunsetPolicy();
        var apiVersion = new ApiVersion( 0.9 );
        var policyManager = new Mock<ISunsetPolicyManager>();

        policyManager.Setup( pm => pm.TryGetPolicy( default, apiVersion, out expected ) ).Returns( true );

        var apiExplorer = new VersionedApiDescriptionProvider(
            policyManager.Object,
            NewModelMetadataProvider(),
            Options.Create( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } ) );

        for ( var i = 0; i < context.Actions.Count; i++ )
        {
            context.Results.Add( new() { ActionDescriptor = context.Actions[i] } );
        }

        // act
        apiExplorer.OnProvidersExecuted( context );

        // assert
        context.Results
               .Where( api => api.GroupName == "v0.9" )
               .Select( api => api.GetSunsetPolicy() )
               .All( policy => policy == expected )
               .Should()
               .BeTrue();
    }

    private static IModelMetadataProvider NewModelMetadataProvider()
    {
        var provider = new Mock<IModelMetadataProvider>();
        var identity = ModelMetadataIdentity.ForType( typeof( string ) );
        var metadata = new Mock<ModelMetadata>( identity ) { CallBase = true };

        provider.Setup( p => p.GetMetadataForType( typeof( string ) ) ).Returns( metadata.Object );

        return provider.Object;
    }
}