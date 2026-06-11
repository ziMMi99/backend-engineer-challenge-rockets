using Rockets.Api.Models;
using Rockets.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<IMessageParser, MessageParser>();
builder.Services.AddSingleton<IRocketStateService, RocketStateService>();


WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/messages", (MessageEnvelope envelope, IMessageHandler handler) => {
	handler.Handle(envelope);
	return Results.Ok();
});

app.MapGet("/rocket/{id:Guid}", (Guid id, IRocketStateService rocketStateService) => {
	RocketState? state = rocketStateService.Get(id);
	return state is null ? Results.NotFound() : Results.Ok(state);
});

app.MapGet("/rockets", (IRocketStateService rocketStateService) => Results.Ok((object?)rocketStateService.GetAll()));

app.MapGet("/health-check", async () => Results.Ok());

app.Run();