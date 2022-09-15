// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.Testing;
using VerifyCS = Microsoft.AspNetCore.Analyzers.Verifiers.CSharpCodeFixVerifier<
    Microsoft.AspNetCore.Analyzers.RouteEmbeddedLanguage.RoutePatternAnalyzer,
    Microsoft.AspNetCore.Analyzers.RouteEmbeddedLanguage.Fixers.RouteParameterAddParameterConstraintFixer>;

namespace Microsoft.AspNetCore.Analyzers.RouteEmbeddedLanguage;

public class RouteParameterAddParameterConstraintFixerTests
{
    [Theory]
    [InlineData("int", "int")]
    [InlineData("int?", "int")]
    [InlineData("uint", "int")]
    [InlineData("DateTime", "datetime")]
    [InlineData("DateTime?", "datetime")]
    [InlineData("Decimal", "decimal")]
    [InlineData("Decimal?", "decimal")]
    [InlineData("decimal", "decimal")]
    [InlineData("decimal?", "decimal")]
    [InlineData("Guid", "guid")]
    [InlineData("Guid?", "guid")]
    [InlineData("double", "double")]
    [InlineData("float", "float")]
    [InlineData("long", "long")]
    [InlineData("ulong", "long")]
    public async Task Controller_ParameterWithoutConstraint_AddConstraint(string type, string constraint)
    {
        // Arrange
        var source = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

class Program
{
    static void Main()
    {
    }
}

public class TestController
{
    [HttpGet(""{|#1:{id}|}"")]
    public object TestAction({|#0:" + type + @" id|})
    {
        return null;
    }
}";

        var fixedSource = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

class Program
{
    static void Main()
    {
    }
}

public class TestController
{
    [HttpGet(""{id:" + constraint + @"}"")]
    public object TestAction(" + type + @" id)
    {
        return null;
    }
}";

        var expectedDiagnostics = new DiagnosticResult(DiagnosticDescriptors.RoutePatternAddParameterConstraint)
            .WithArguments("id")
            .WithLocation(0)
            .WithLocation(1);

        // Act & Assert
        await VerifyCS.VerifyCodeFixAsync(source, expectedDiagnostics, fixedSource);
    }

    [Fact]
    public async Task Controller_ParameterWithoutTypeConstraint_AddConstraintToExistingConstraint()
    {
        // Arrange
        var source = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

class Program
{
    static void Main()
    {
    }
}

public class TestController
{
    [HttpGet(""{|#1:{id:min(10)}|}"")]
    public object TestAction({|#0:int id|})
    {
        return null;
    }
}";

        var fixedSource = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

class Program
{
    static void Main()
    {
    }
}

public class TestController
{
    [HttpGet(""{id:int:min(10)}"")]
    public object TestAction(int id)
    {
        return null;
    }
}";

        var expectedDiagnostics = new DiagnosticResult(DiagnosticDescriptors.RoutePatternAddParameterConstraint)
            .WithArguments("id")
            .WithLocation(0)
            .WithLocation(1);

        // Act & Assert
        await VerifyCS.VerifyCodeFixAsync(source, expectedDiagnostics, fixedSource);
    }

    [Fact]
    public async Task MapGet_ParameterWithoutConstraint_AddConstraint()
    {
        // Arrange
        var source = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        EndpointRouteBuilderExtensions.MapGet(null, @""{|#1:{id:min(10)}|}"", ({|#0:int id|}) => ""test"");
    }
}
";

        var fixedSource = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        EndpointRouteBuilderExtensions.MapGet(null, @""{id:int:min(10)}"", (int id) => ""test"");
    }
}
";

        var expectedDiagnostics = new DiagnosticResult(DiagnosticDescriptors.RoutePatternAddParameterConstraint)
            .WithArguments("id")
            .WithLocation(0)
            .WithLocation(1);

        // Act & Assert
        await VerifyCS.VerifyCodeFixAsync(source, expectedDiagnostics, fixedSource);
    }

    [Fact]
    public async Task MapGet_Multiple_ParameterWithoutConstraint_AddConstraint()
    {
        // Arrange
        var source = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        EndpointRouteBuilderExtensions.MapGet(null, @""{|#1:{id:min(10)}|}/{|#3:{date}|}"", ({|#0:int id|}, {|#2:DateTime date|}) => ""test"");
    }
}
";

        var fixedSource = @"
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        EndpointRouteBuilderExtensions.MapGet(null, @""{id:int:min(10)}/{date:datetime}"", (int id, DateTime date) => ""test"");
    }
}
";

        var expectedDiagnostics = new[]
        {
            new DiagnosticResult(DiagnosticDescriptors.RoutePatternAddParameterConstraint)
                .WithArguments("id")
                .WithLocation(0)
                .WithLocation(1),
            new DiagnosticResult(DiagnosticDescriptors.RoutePatternAddParameterConstraint)
                .WithArguments("date")
                .WithLocation(2)
                .WithLocation(3),
        };

        // Act & Assert
        await VerifyCS.VerifyCodeFixAsync(source, expectedDiagnostics, fixedSource, expectedIterations: 2);
    }
}
