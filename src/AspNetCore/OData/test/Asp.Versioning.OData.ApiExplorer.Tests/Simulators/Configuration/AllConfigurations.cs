﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for all configurations.
/// </summary>
public class AllConfigurations : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        builder.Function( "GetSalesTaxRate" ).Returns<double>().Parameter<int>( "PostalCode" );
    }
}