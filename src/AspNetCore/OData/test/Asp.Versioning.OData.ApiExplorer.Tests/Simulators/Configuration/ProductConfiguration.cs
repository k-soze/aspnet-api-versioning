﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for products.
/// </summary>
public class ProductConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        if ( apiVersion < ApiVersions.V3 )
        {
            return;
        }

        var product = builder.EntitySet<Product>( "Products" ).EntityType.HasKey( p => p.Id );
    }
}