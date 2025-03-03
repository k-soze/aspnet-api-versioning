﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly ApiVersionDescriptionCollection collection;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="endpointDataSource">The <see cref="EndpointDataSource">data source</see> for <see cref="Endpoint">endpoints</see>.</param>
    /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see>
    /// used to enumerate the actions within an application.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public DefaultApiVersionDescriptionProvider(
        EndpointDataSource endpointDataSource,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        ISunsetPolicyManager sunsetPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        collection = new( this, endpointDataSource, actionDescriptorCollectionProvider );
        SunsetPolicyManager = sunsetPolicyManager;
        options = apiExplorerOptions;
    }

    /// <summary>
    /// Gets the manager used to resolve sunset policies.
    /// </summary>
    /// <value>The associated <see cref="ISunsetPolicyManager">sunset policy manager</see>.</value>
    protected ISunsetPolicyManager SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
    protected ApiExplorerOptions Options => options.Value;

    /// <inheritdoc />
    public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => collection.Items;

    /// <summary>
    /// Provides a list of API version descriptions from a list of application API version metadata.
    /// </summary>
    /// <param name="metadata">The <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionMetadata">API version metadata</see>
    /// within the application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> Describe( IReadOnlyList<ApiVersionMetadata> metadata )
    {
        if ( metadata == null )
        {
            throw new ArgumentNullException( nameof( metadata ) );
        }

        var descriptions = new List<ApiVersionDescription>( capacity: metadata.Count );
        var supported = new HashSet<ApiVersion>();
        var deprecated = new HashSet<ApiVersion>();

        BucketizeApiVersions( metadata, supported, deprecated );
        AppendDescriptions( descriptions, supported, deprecated: false );
        AppendDescriptions( descriptions, deprecated, deprecated: true );

        return descriptions.OrderBy( d => d.ApiVersion ).ToArray();
    }

    private void BucketizeApiVersions( IReadOnlyList<ApiVersionMetadata> metadata, ISet<ApiVersion> supported, ISet<ApiVersion> deprecated )
    {
        var declared = new HashSet<ApiVersion>();
        var advertisedSupported = new HashSet<ApiVersion>();
        var advertisedDeprecated = new HashSet<ApiVersion>();

        for ( var i = 0; i < metadata.Count; i++ )
        {
            var model = metadata[i].Map( Explicit | Implicit );
            var versions = model.DeclaredApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                declared.Add( versions[j] );
            }

            versions = model.SupportedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                supported.Add( version );
                advertisedSupported.Add( version );
            }

            versions = model.DeprecatedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                deprecated.Add( version );
                advertisedDeprecated.Add( version );
            }
        }

        advertisedSupported.ExceptWith( declared );
        advertisedDeprecated.ExceptWith( declared );
        supported.ExceptWith( advertisedSupported );
        deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );

        if ( supported.Count == 0 && deprecated.Count == 0 )
        {
            supported.Add( Options.DefaultApiVersion );
        }
    }

    private void AppendDescriptions( ICollection<ApiVersionDescription> descriptions, IEnumerable<ApiVersion> versions, bool deprecated )
    {
        foreach ( var version in versions )
        {
            var groupName = version.ToString( Options.GroupNameFormat, CurrentCulture );
            var sunsetPolicy = SunsetPolicyManager.TryGetPolicy( version, out var policy ) ? policy : default;
            descriptions.Add( new( version, groupName, deprecated, sunsetPolicy ) );
        }
    }

    private sealed class ApiVersionDescriptionCollection
    {
        private readonly object syncRoot = new();
        private readonly DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider;
        private readonly EndpointApiVersionMetadataCollection endpoints;
        private readonly ActionApiVersionMetadataCollection actions;
        private IReadOnlyList<ApiVersionDescription>? items;
        private long version;

        public ApiVersionDescriptionCollection(
            DefaultApiVersionDescriptionProvider apiVersionDescriptionProvider,
            EndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            this.apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            endpoints = new( endpointDataSource );
            actions = new( actionDescriptorCollectionProvider );
        }

        public IReadOnlyList<ApiVersionDescription> Items
        {
            get
            {
                if ( items is not null && version == CurrentVersion )
                {
                    return items;
                }

                lock ( syncRoot )
                {
                    var (items1, version1) = endpoints;
                    var (items2, version2) = actions;
                    var currentVersion = ComputeVersion( version1, version2 );

                    if ( items is not null && version == currentVersion )
                    {
                        return items;
                    }

                    var capacity = items1.Count + items2.Count;
                    var metadata = new List<ApiVersionMetadata>( capacity );

                    for ( var i = 0; i < items1.Count; i++ )
                    {
                        metadata.Add( items1[i] );
                    }

                    for ( var i = 0; i < items2.Count; i++ )
                    {
                        metadata.Add( items2[i] );
                    }

                    items = apiVersionDescriptionProvider.Describe( metadata );
                    version = currentVersion;
                }

                return items;
            }
        }

        private long CurrentVersion
        {
            get
            {
                lock ( syncRoot )
                {
                    return ComputeVersion( endpoints.Version, actions.Version );
                }
            }
        }

        private static long ComputeVersion( int version1, int version2 ) => ( ( (long) version1 ) << 32 ) | (long) version2;
    }

    private sealed class EndpointApiVersionMetadataCollection
    {
        private readonly object syncRoot = new();
        private readonly EndpointDataSource endpointDataSource;
        private List<ApiVersionMetadata>? list;
        private int version;
        private int currentVersion;

        public EndpointApiVersionMetadataCollection( EndpointDataSource endpointDataSource )
        {
            this.endpointDataSource = endpointDataSource ?? throw new ArgumentNullException( nameof( endpointDataSource ) );
            ChangeToken.OnChange( endpointDataSource.GetChangeToken, IncrementVersion );
        }

        public int Version => version;

        public IReadOnlyList<ApiVersionMetadata> Items
        {
            get
            {
                if ( list is not null && version == currentVersion )
                {
                    return list;
                }

                lock ( syncRoot )
                {
                    if ( list is not null && version == currentVersion )
                    {
                        return list;
                    }

                    var endpoints = endpointDataSource.Endpoints;

                    if ( list == null )
                    {
                        list = new( capacity: endpoints.Count );
                    }
                    else
                    {
                        list.Clear();
                        list.Capacity = endpoints.Count;
                    }

                    for ( var i = 0; i < endpoints.Count; i++ )
                    {
                        if ( endpoints[i].Metadata.GetMetadata<ApiVersionMetadata>() is ApiVersionMetadata item )
                        {
                            list.Add( item );
                        }
                    }

                    version = currentVersion;
                }

                return list;
            }
        }

        public void Deconstruct( out IReadOnlyList<ApiVersionMetadata> items, out int version )
        {
            lock ( syncRoot )
            {
                version = this.version;
                items = Items;
            }
        }

        private void IncrementVersion()
        {
            lock ( syncRoot )
            {
                currentVersion++;
            }
        }
    }

    private sealed class ActionApiVersionMetadataCollection
    {
        private readonly object syncRoot = new();
        private readonly IActionDescriptorCollectionProvider provider;
        private List<ApiVersionMetadata>? list;
        private int version;

        public ActionApiVersionMetadataCollection( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider ) =>
            provider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException( nameof( actionDescriptorCollectionProvider ) );

        public int Version => version;

        public IReadOnlyList<ApiVersionMetadata> Items
        {
            get
            {
                var collection = provider.ActionDescriptors;

                if ( list is not null && collection.Version == version )
                {
                    return list;
                }

                lock ( syncRoot )
                {
                    if ( list is not null && collection.Version == version )
                    {
                        return list;
                    }

                    var actions = collection.Items;

                    if ( list == null )
                    {
                        list = new( capacity: actions.Count );
                    }
                    else
                    {
                        list.Clear();
                        list.Capacity = actions.Count;
                    }

                    for ( var i = 0; i < actions.Count; i++ )
                    {
                        list.Add( actions[i].GetApiVersionMetadata() );
                    }

                    version = collection.Version;
                }

                return list;
            }
        }

        public void Deconstruct( out IReadOnlyList<ApiVersionMetadata> items, out int version )
        {
            lock ( syncRoot )
            {
                version = this.version;
                items = Items;
            }
        }
    }
}