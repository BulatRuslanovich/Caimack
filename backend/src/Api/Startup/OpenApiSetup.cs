using System.Globalization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;


namespace Api.Startup;

public static class OpenApiSetup
{
	private const string DocPath = "/openapi/v1.json";

	public static IServiceCollection AddApiOpenApi(this IServiceCollection s)
	{
		s.AddOpenApi("v1", o =>
		{
			o.AddDocumentTransformer(DescribeBearerAuth);
			o.AddOperationTransformer(DescribeAuthFail);
		});

		return s;
	}

	public static WebApplication MapApiOpenApi(this WebApplication app)
	{
		app.MapOpenApi(DocPath).AllowAnonymous();

		app.MapScalarApiReference("docs",
			o => o.WithTitle("Caimack API").WithOpenApiRoutePattern(DocPath).DisableDefaultFonts().DisableTelemetry()
				.DisableAgent().DisableMcp()).AllowAnonymous();



		return app;
	}

	private static Task DescribeBearerAuth(OpenApiDocument doc, OpenApiDocumentTransformerContext cnx,
		CancellationToken ct)
	{
		doc.Info.Title = "Caimack API";
		doc.Info.Version = "v1";

		// TODO: bearer token
		return Task.CompletedTask;
	}

	private static Task DescribeAuthFail(OpenApiOperation op, OpenApiOperationTransformerContext cnx,
		CancellationToken ct)
	{
		// TODO: roles

		return  Task.CompletedTask;
	}

	private static void AddResponse(OpenApiOperation op, int code, string desc)
	{
		op.Responses ??= [];

		var key = code.ToString(CultureInfo.InvariantCulture);

		if (!op.Responses.ContainsKey(key))
		{
			op.Responses[key] = new OpenApiResponse { Description = desc };
		}
	}
}
