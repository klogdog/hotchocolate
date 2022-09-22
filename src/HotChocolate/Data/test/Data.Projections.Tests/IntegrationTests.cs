using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookieCrumble;
using HotChocolate.Execution;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.Extensions.DependencyInjection;

namespace HotChocolate.Data;

public class IntegrationTests
{
    [Fact]
    public async Task Projection_Should_NotBreakProjections_When_ExtensionsFieldRequested()
    {
        // arrange
        // act
        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddTypeExtension<FooExtensions>()
            .AddProjections()
            .BuildRequestExecutorAsync();

        // assert
        var result = await executor.ExecuteAsync(@"
            {
                foos {
                    bar
                    baz
                }
            }
            ");

        result.MatchSnapshot();
    }

    [Fact]
    public async Task Projection_Should_NotBreakProjections_When_ExtensionsListRequested()
    {
        // arrange
        // act
        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddTypeExtension<FooExtensions>()
            .AddProjections()
            .BuildRequestExecutorAsync();

        // assert
        var result = await executor.ExecuteAsync(@"
            {
                foos {
                    bar
                    qux
                }
            }
            ");

        result.MatchSnapshot();
    }

    [Fact]
    public async Task Projection_Should_NotBreakProjections_When_ExtensionsObjectListRequested()
    {
        // arrange
        // act
        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddTypeExtension<FooExtensions>()
            .AddProjections()
            .BuildRequestExecutorAsync();

        // assert
        var result = await executor.ExecuteAsync(@"
            {
                foos {
                    bar
                    nestedList {
                        bar
                    }
                }
            }
            ");

        result.MatchSnapshot();
    }

    [Fact]
    public async Task Projection_Should_NotBreakProjections_When_ExtensionsObjectRequested()
    {
        // arrange
        // act
        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddTypeExtension<FooExtensions>()
            .AddProjections()
            .BuildRequestExecutorAsync();

        // assert
        var result = await executor.ExecuteAsync(@"
            {
                foos {
                    bar
                    nested {
                        bar
                    }
                }
            }
            ");

        result.MatchSnapshot();
    }

    [Fact]
    public async Task Node_Resolver_With_SingleOrDefault_Schema()
    {
        var schema = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<QueryWithNodeResolvers>()
            .AddObjectType<Foo>(d => d.ImplementsNode().IdField(t => t.Bar))
            .AddGlobalObjectIdentification()
            .AddProjections()
            .BuildSchemaAsync();

        schema.MatchSnapshot();
    }

    [Fact]
    public async Task Node_Resolver_With_SingleOrDefault()
    {
        var executor = await new ServiceCollection()
            .AddGraphQL()
            .AddQueryType<QueryWithNodeResolvers>()
            .AddObjectType<Foo>(d => d.ImplementsNode().IdField(t => t.Bar))
            .AddGlobalObjectIdentification()
            .AddProjections()
            .BuildRequestExecutorAsync();

        var result = await executor.ExecuteAsync(@"{ node(id: ""Rm9vCmRB"") { id __typename } }");

        result.MatchSnapshot();
    }
}

public class Query
{
    [UseProjection]
    public IQueryable<Foo> Foos => new Foo[]
    {
        new() { Bar = "A" },
        new() { Bar = "B" }
    }.AsQueryable();
}

[ExtendObjectType(typeof(Foo))]
public class FooExtensions
{
    public string Baz => "baz";

    public IEnumerable<string> Qux => new[]
    {
            "baz"
        };

    public IEnumerable<Foo> NestedList => new[]
    {
            new Foo() { Bar = "C" }
        };

    public Foo Nested => new() { Bar = "C" };
}

public class Foo
{
    public string? Bar { get; set; }
}

public class QueryWithNodeResolvers
{
    [UseProjection]
    public IQueryable<Foo> All()
        => new Foo[]
        {
            new() { Bar = "A" },
        }.AsQueryable();

    [NodeResolver]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Foo> GetById(string id)
        => new Foo[]
        {
            new() { Bar = "A" },
        }.AsQueryable();
}
