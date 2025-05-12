using System;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasMany(a => a.Blogs).WithOne(b => b.Author).HasForeignKey(b => b.AuthorId);
            entity.HasKey(a => a.Id);
            entity.ToTable("__Authors");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.ToTable("__Blogs");
        });

        base.OnModelCreating(modelBuilder);
    }
}
