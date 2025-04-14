namespace SnapClicker.Data;

public class SnapClickerDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
        string configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapClicker");
        var dbPath = Path.Combine(configDirectory, "data.db");

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecordedAction>()
            .HasOne(r => r.Preset)
            .WithMany(p => p.RecordedActions)
            .HasForeignKey(r => r.PresetId)
            .OnDelete(DeleteBehavior.Cascade); 

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Preset> Presets { get; set; }
    public DbSet<RecordedAction> RecordedActions { get; set; }
}