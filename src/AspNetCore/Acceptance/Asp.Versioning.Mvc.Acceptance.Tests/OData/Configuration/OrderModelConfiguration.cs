﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Asp.Versioning.OData.Configuration;

using Asp.Versioning.OData.Models;
using Microsoft.OData.ModelBuilder;

public class OrderModelConfiguration : IModelConfiguration
{
    private readonly ApiVersion supportedApiVersion;

    public OrderModelConfiguration() { }

    public OrderModelConfiguration( ApiVersion supportedApiVersion ) => this.supportedApiVersion = supportedApiVersion;

    private static EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
    {
        var order = builder.EntitySet<Order>( "Orders" ).EntityType;
        order.HasKey( p => p.Id );
        return order;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        if ( supportedApiVersion == null || supportedApiVersion == apiVersion )
        {
            ConfigureCurrent( builder );
        }
    }
}