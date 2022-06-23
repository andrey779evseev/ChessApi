using Microsoft.EntityFrameworkCore;

namespace ChessApi.Data;

public class ChessContext: DbContext
{
    public ChessContext():base(){}
    public ChessContext(DbContextOptions<ChessContext> options):base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<WaiterForGame> WaitersForGame { get; set; }
    public DbSet<Game> Games { get; set; }
        
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("User ID = postgres;Password=c8tf6q95ddp7;Server=db.cjgjsrwxesxkebubhvua.supabase.co;Port=5432;Database=postgres;Integrated Security=true; Pooling=true;");
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}