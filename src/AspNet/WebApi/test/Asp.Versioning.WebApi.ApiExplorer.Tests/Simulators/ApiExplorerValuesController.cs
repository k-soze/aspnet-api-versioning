﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using System.Web.Http;
using System.Web.Http.Description;

public class ApiExplorerValuesController : ApiController
{
    public void Get() { }

    [ApiExplorerSettings( IgnoreApi = true )]
    public void Post() { }
}