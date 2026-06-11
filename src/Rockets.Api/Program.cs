using Rockets.Api.Models;
using Rockets.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<IMessageParser, MessageParser>();
builder.Services.AddSingleton<IRocketStateService, RocketStateService>();


WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/messages", (MessageEnvelope? envelope, IMessageHandler handler) => {
	if (envelope is null) {
		return Results.BadRequest("Supplied message was invalid");
	}
	
	handler.Handle(envelope);
	return Results.Ok();
});

app.MapGet("/rocket/{id:Guid}", (Guid id, IRocketStateService rocketStateService) => {
	RocketState? state = rocketStateService.Get(id);
	return state is null ? Results.NotFound() : Results.Ok(state);
});

app.MapGet("/rockets", (IRocketStateService rocketStateService) => Results.Ok((object?)rocketStateService.GetAll()));

app.MapGet("/health-check", () => {
	try {
		return Task.FromResult(Results.Ok());
	}
	catch (Exception exception) {
		return Task.FromResult(Results.BadRequest(exception));
	}
});

app.Run();