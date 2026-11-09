using Moneta.Domain.Auth;
using Moneta.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Infra.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser) : DbContext(options)
{
    // Read by the query filters below on every query execution (EF parameterizes DbContext
    // members), so one model serves every user. Unauthenticated + non-system sees nothing — the
    // explicit null check matters: EF's C# null semantics would otherwise turn
    // `UserId == null` into `IS NULL` and expose rows not yet claimed by any account.
    private Guid? CurrentUserId => currentUser.UserId;
    private bool IsSystem => currentUser.IsSystem;

    public DbSet<User> Users => Set<User>();

    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineExecution> RoutineExecutions => Set<RoutineExecution>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Module> Modules => Set<Module>();

    public DbSet<DateControlParty> DateControlParties => Set<DateControlParty>();
    public DbSet<DateControlCategory> DateControlCategories => Set<DateControlCategory>();
    public DbSet<DateControlSource> DateControlSources => Set<DateControlSource>();
    public DbSet<DateControlEntry> DateControlEntries => Set<DateControlEntry>();
    public DbSet<DateControlEntryAttachment> DateControlEntryAttachments => Set<DateControlEntryAttachment>();

    public DbSet<TimeControlParty> TimeControlParties => Set<TimeControlParty>();
    public DbSet<TimeControlType> TimeControlTypes => Set<TimeControlType>();
    public DbSet<TimeControlItem> TimeControlItems => Set<TimeControlItem>();
    public DbSet<TimeControlOccurrence> TimeControlOccurrences => Set<TimeControlOccurrence>();

    public DbSet<DateTimeControlEntry> DateTimeControlEntries => Set<DateTimeControlEntry>();

    public DbSet<DateControlReminderDispatch> DateControlReminderDispatches => Set<DateControlReminderDispatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenancy(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.FullName)
            .HasMaxLength(150);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(200);

        modelBuilder.Entity<User>()
            .Property(u => u.Login)
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<Module>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Routine>()
            .Property(r => r.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<Routine>()
            .HasOne(r => r.Module)
            .WithMany()
            .HasForeignKey(r => r.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoutineExecution>()
            .HasOne(e => e.Routine)
            .WithMany(r => r.Executions)
            .HasForeignKey(e => e.RoutineId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Execution)
            .WithMany(e => e.Notifications)
            .HasForeignKey(n => n.ExecutionId);

        modelBuilder.Entity<Module>()
            .Property(m => m.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Module>()
            .Property(m => m.Slug)
            .HasMaxLength(100);

        modelBuilder.Entity<Module>()
            .HasIndex(m => new { m.UserId, m.Slug })
            .IsUnique();

        // DateControl (era Financeiro)

        modelBuilder.Entity<DateControlSource>()
            .Property(s => s.Description)
            .HasMaxLength(200);

        modelBuilder.Entity<DateControlSource>()
            .HasOne(s => s.Module)
            .WithMany()
            .HasForeignKey(s => s.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DateControlCategory>().Property(c => c.Name).HasMaxLength(100);
        modelBuilder.Entity<DateControlCategory>().Property(c => c.Icon).HasMaxLength(50);

        modelBuilder.Entity<DateControlCategory>()
            .HasOne(c => c.Module)
            .WithMany()
            .HasForeignKey(c => c.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DateControlSource>()
            .HasOne(s => s.DateControlCategory)
            .WithMany(c => c.Sources)
            .HasForeignKey(s => s.DateControlCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DateControlParty>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<DateControlParty>()
            .HasOne(p => p.Module)
            .WithMany()
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DateControlEntry>()
            .Property(e => e.Amount)
            .HasPrecision(12, 2);

        modelBuilder.Entity<DateControlEntry>()
            .HasOne(e => e.Module)
            .WithMany()
            .HasForeignKey(e => e.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DateControlEntry>()
            .HasOne(e => e.DateControlSource)
            .WithMany(s => s.Entries)
            .HasForeignKey(e => e.DateControlSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DateControlEntry>()
            .HasOne(e => e.DateControlParty)
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.DateControlPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DateControlEntry>()
            .Property(e => e.Notes)
            .HasMaxLength(DateControlEntry.NotesMaxLength);

        modelBuilder.Entity<DateControlEntryAttachment>()
            .Property(a => a.FileName)
            .HasMaxLength(255);

        modelBuilder.Entity<DateControlEntryAttachment>()
            .Property(a => a.ContentType)
            .HasMaxLength(100);

        modelBuilder.Entity<DateControlEntryAttachment>()
            .HasOne(a => a.DateControlEntry)
            .WithMany(e => e.Attachments)
            .HasForeignKey(a => a.DateControlEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // TimeControl (era Vida Saudável)

        modelBuilder.Entity<TimeControlParty>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<TimeControlParty>()
            .HasOne(p => p.Module)
            .WithMany()
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeControlType>().Property(t => t.Name).HasMaxLength(100);
        modelBuilder.Entity<TimeControlType>().Property(t => t.Icon).HasMaxLength(50);

        modelBuilder.Entity<TimeControlType>()
            .HasOne(t => t.Module)
            .WithMany()
            .HasForeignKey(t => t.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeControlItem>()
            .HasOne(i => i.TimeControlType)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.TimeControlTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeControlItem>()
            .Property(i => i.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<TimeControlItem>()
            .HasOne(i => i.Module)
            .WithMany()
            .HasForeignKey(i => i.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeControlItem>()
            .HasOne(i => i.TimeControlParty)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.TimeControlPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeControlOccurrence>()
            .HasOne(o => o.TimeControlItem)
            .WithMany(i => i.Occurrences)
            .HasForeignKey(o => o.TimeControlItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // DateTimeControl (novo, Agenda)

        modelBuilder.Entity<DateTimeControlEntry>()
            .Property(e => e.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<DateTimeControlEntry>()
            .HasOne(e => e.Module)
            .WithMany()
            .HasForeignKey(e => e.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        // DateControlReminderDispatch

        modelBuilder.Entity<DateControlReminderDispatch>()
            .HasOne(d => d.DateControlEntry)
            .WithMany()
            .HasForeignKey(d => d.DateControlEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DateControlReminderDispatch>()
            .HasIndex(d => new { d.DateControlEntryId, d.RuleKind, d.FiredAt });
    }

    /// <summary>
    /// Row-level isolation: every user-owned entity is visible only when its Module belongs to the
    /// current user (or the caller runs as system). Applied centrally so no repository can forget
    /// it — GetById, ExecuteUpdate/Delete and includes all go through these filters.
    /// </summary>
    private void ConfigureTenancy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Module>().HasQueryFilter(m => IsSystem || (CurrentUserId != null && m.UserId == CurrentUserId));

        modelBuilder.Entity<DateControlCategory>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<TimeControlType>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<DateControlParty>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<DateControlSource>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<DateControlEntry>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<DateControlEntryAttachment>()
            .HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.DateControlEntry.Module.UserId == CurrentUserId));
        modelBuilder.Entity<DateControlReminderDispatch>()
            .HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.DateControlEntry.Module.UserId == CurrentUserId));

        modelBuilder.Entity<TimeControlParty>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<TimeControlItem>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<TimeControlOccurrence>()
            .HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.TimeControlItem.Module.UserId == CurrentUserId));

        modelBuilder.Entity<DateTimeControlEntry>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));

        modelBuilder.Entity<Routine>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Module.UserId == CurrentUserId));
        modelBuilder.Entity<RoutineExecution>().HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Routine.Module.UserId == CurrentUserId));
        modelBuilder.Entity<Notification>()
            .HasQueryFilter(x => IsSystem || (CurrentUserId != null && x.Execution.Routine.Module.UserId == CurrentUserId));
    }
}
