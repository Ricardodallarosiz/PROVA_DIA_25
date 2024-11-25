using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();


app.MapGet("/", () => "Prova A1");

//ENDPOINTS DE CATEGORIA
//GET: http://localhost:5273/api/categoria/listar
app.MapGet("/api/categoria/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Categorias.Any())
    {
        return Results.Ok(ctx.Categorias.ToList());
    }
    return Results.NotFound("Nenhuma categoria encontrada");
});

//POST: http://localhost:5273/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    ctx.Categorias.Add(categoria);
    ctx.SaveChanges();
    return Results.Created("", categoria);
});

//ENDPOINTS DE TAREFA
//GET: http://localhost:5273/api/tarefas/listar
app.MapGet("/api/tarefa/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        return Results.Ok(ctx.Tarefas.Include(x => x.Categoria).ToList());
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

//POST: http://localhost:5273/api/tarefas/cadastrar
app.MapPost("/api/tarefa/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    Categoria? categoria = ctx.Categorias.Find(tarefa.CategoriaId);
    if (categoria == null)
    {
        return Results.NotFound("Categoria não encontrada");
    }
    tarefa.Categoria = categoria;
    ctx.Tarefas.Add(tarefa);
    ctx.SaveChanges();
    return Results.Created("", tarefa);
});

//PUT: http://localhost:5273/tarefas/alterar/{id}
app.MapPut("/api/tarefa/alterar/{id}", async (int id, AppDataContext ctx) =>
{
    var tarefa = await ctx.Tarefas.FindAsync(id);
    if (tarefa == null)
    {
        return Results.NotFound("Tarefa não encontrada.");
    }


    tarefa.Status = tarefa.Status switch
    {
        "Não iniciada" => "Em andamento",
        "Em andamento" => "Concluída",
        _ => tarefa.Status
    };

    ctx.Tarefas.Update(tarefa);
    await ctx.SaveChangesAsync();
    return Results.Ok(tarefa);
});


//GET: http://localhost:5273/tarefas/naoconcluidas
app.MapGet("/api/tarefa/naoconcluidas", async (AppDataContext ctx) =>
{
    var tarefas = await ctx.Tarefas
        .Include(t => t.Categoria)
        .Where(t => t.Status != "Concluída")
        .ToListAsync();
    return tarefas.Any() ? Results.Ok(tarefas) : Results.NotFound();
});

//GET: http://localhost:5273/tarefas/concluidas
app.MapGet("/api/tarefa/concluidas", async (AppDataContext ctx) =>
{
    var tarefas = await ctx.Tarefas
        .Include(t => t.Categoria)
        .Where(t => t.Status == "Concluída")
        .ToListAsync();
    return tarefas.Any() ? Results.Ok(tarefas) : Results.NotFound();
});
app.Run();
