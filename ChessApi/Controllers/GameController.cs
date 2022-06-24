using System.Net.WebSockets;
using ChessApi.Controllers.Requests;
using ChessApi.Controllers.Responses;
using ChessApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChessApi.Controllers;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    [HttpPost("start-waiting")]
    public async Task<ActionResult> StartWaiting(WaitingRequestBodyType body)
    {
        await using var ctx = new ChessContext();
        var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.Id);
        if (user == null)
            return NotFound("Пользователь с таким id не найден");
        await ctx.WaitersForGame.AddAsync(new WaiterForGame(user.Id, user.Name));
        await ctx.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("stop-waiting")]
    public async Task<ActionResult> StopWaiting(WaitingRequestBodyType body)
    {
        await using var ctx = new ChessContext();
        var waiter = await ctx.WaitersForGame.FirstOrDefaultAsync(x => x.Id == body.Id);
        if (waiter == null)
            return NotFound($"Пользователь с таким id не найден = {body.Id}");
        ctx.WaitersForGame.Remove(waiter);
        await ctx.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("is-waiting/{id}")]
    public async Task<ActionResult> IsWaiting(string id)
    {
        await using var ctx = new ChessContext();
        var waiter = await ctx.WaitersForGame.FirstOrDefaultAsync(x => x.Id == id);
        if (waiter == null)
            return Ok(false);
        return Ok(true);
    }
    [HttpGet("waiters")]
    public async Task<ActionResult> GetWaiters()
    {
        await using var ctx = new ChessContext();
        var waiters = await ctx.WaitersForGame.ToListAsync();
        // foreach (var webSocket in ChessWebSocketMiddleware.Sockets)
        // {
        //     await ChessWebSocketMiddleware.SendStringAsync(webSocket.Value, JsonConvert.SerializeObject(new WebSocketMessage("asd", "asdd", "sad", WebSocketMessage.EnumWebSocketMessageType.ChatMessage, null, "dsgdgsdfg")));
        // }
        return Ok(waiters);
    }
    [HttpGet("rates")]
    public async Task<ActionResult> GetRates()
    {
        await using var ctx = new ChessContext();
        var users = await ctx.Users
            .Select(x => new {x.Id, x.Name})
            .ToListAsync();
        var rates = new List<RateResponseBodyType>();
        foreach (var user in users)
        {
            var count = await ctx.Games.CountAsync(x => x.WinnerId == user.Id);
            rates.Add(new RateResponseBodyType(user.Name, user.Id, count));
        }
        return Ok(rates);
    }
    [HttpPost("start-game")]
    public async Task<ActionResult> StartGame(StartGameRequestBodyType body)
    {
        await using var ctx = new ChessContext();
        var waiter = await ctx.WaitersForGame.FirstOrDefaultAsync(x => x.Id == body.FirstOpponentId);
        if (waiter != null)
            ctx.WaitersForGame.Remove(waiter);
        var waiter2 = await ctx.WaitersForGame.FirstOrDefaultAsync(x => x.Id == body.SecondOpponentId);
        if (waiter2 != null)
            ctx.WaitersForGame.Remove(waiter2);
        var game = new Game(body.FirstOpponentId, body.SecondOpponentId, Game.GameStatus.InProgress, null, null);
        await ctx.Games.AddAsync(game);
        await ctx.SaveChangesAsync();
        foreach (var socket in ChessWebSocketMiddleware.Sockets)
        {
            if (socket.Value.State != WebSocketState.Open) continue;
            if (socket.Key == body.FirstOpponentId)
            {
                var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.SecondOpponentId);
                if (user == null)
                    return NotFound($"Пользователь с таким id не найден = {body.SecondOpponentId}");
                var obj = new WebSocketMessage(body.FirstOpponentId, body.SecondOpponentId, user.Name, WebSocketMessage.EnumWebSocketMessageType.StartGame, body.IsWhite, body.Board);
                await ChessWebSocketMiddleware.SendStringAsync(socket.Value, JsonConvert.SerializeObject(obj), CancellationToken.None);
            } else if (socket.Key == body.SecondOpponentId)
            {
                var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.FirstOpponentId);
                if (user == null)
                    return NotFound($"Пользователь с таким id не найден = {body.FirstOpponentId}");
                var obj = new WebSocketMessage(
                    body.SecondOpponentId, 
                    body.FirstOpponentId, 
                    user.Name, 
                    WebSocketMessage.EnumWebSocketMessageType.StartGame, 
                    !body.IsWhite, 
                    body.Board
                );
                await ChessWebSocketMiddleware.SendStringAsync(socket.Value, JsonConvert.SerializeObject(obj), CancellationToken.None);
            }
        }
        return Ok();
    }
    
    [HttpPost("stop-game")]
    public async Task<ActionResult> StopGame(StopGameRequestBodyType body)
    {
        await using var ctx = new ChessContext();
        var game = await ctx.Games.FirstOrDefaultAsync(x => x.FirstOpponentId == body.FirstOpponentId && x.SecondOpponentId == body.SecondOpponentId && x.Status == Game.GameStatus.InProgress);
        if (game == null)
            return NotFound();
        var winner = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.WinnerId);
        if (winner == null)
            return NotFound();
        game.WinnerId = body.WinnerId;
        game.WinnerName = winner.Name;
        game.Status = Game.GameStatus.Finished;
        ctx.Games.Update(game);
        await ctx.SaveChangesAsync();
        foreach (var socket in ChessWebSocketMiddleware.Sockets)
        {
            if (socket.Value.State == WebSocketState.Open) continue;
            if (socket.Key == body.FirstOpponentId)
            {
                var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.FirstOpponentId);
                if (user == null)
                    return NotFound($"Пользователь с таким id не найден = {body.FirstOpponentId}");
                var obj = new WebSocketMessage(body.SecondOpponentId, body.FirstOpponentId, user.Name, WebSocketMessage.EnumWebSocketMessageType.EndGame, null, null, body.WinnerId, winner.Name);
                await ChessWebSocketMiddleware.SendStringAsync(socket.Value, JsonConvert.SerializeObject(obj), CancellationToken.None);
            } else if (socket.Key == body.SecondOpponentId)
            {
                var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == body.SecondOpponentId);
                if (user == null)
                    return NotFound($"Пользователь с таким id не найден = {body.SecondOpponentId}");
                var obj = new WebSocketMessage(body.FirstOpponentId, body.SecondOpponentId, user.Name, WebSocketMessage.EnumWebSocketMessageType.EndGame, null, null, body.WinnerId, winner.Name);
                await ChessWebSocketMiddleware.SendStringAsync(socket.Value, JsonConvert.SerializeObject(obj), CancellationToken.None);
            }
        }
        return Ok();
    }
}