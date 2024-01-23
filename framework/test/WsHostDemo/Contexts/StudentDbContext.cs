using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Silky.EntityFrameworkCore.Contexts.Attributes;
using Silky.EntityFrameworkCore.Extras.Contexts;

namespace WsHostDemo.Contexts;

[AppDbContext("StudentDbContext", DbProvider.MySql)]
public class StudentDbContext : SilkyDbContext<StudentDbContext>
{
    public StudentDbContext([NotNull] [ItemNotNull] DbContextOptions<StudentDbContext> options) : base(options)
    {
    }
    
    public DbSet<Student> Students { get; set; }
}