using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ZPIServer.Models;

/// <summary>
/// Klasa z pakietu EntityFramework Core, która pozwala na przeprowadzanie operacji CRUD (Create, Read, Update, Delete) na bazie danych w przyjazny i programowalny sposób.<br/>
/// Przykład użycia:
/// <code>
/// using (var context = new SchoolDbContext())
/// {
///    //creates db if not exists 
///    context.Database.EnsureCreated();
///
///    //create entity objects
///    var grd1 = new Grade() { GradeName = "1st Grade" };
///    var std1 = new Student() { FirstName = "Yash", LastName = "Malhotra", Grade = grd1 };
///
///    //add entitiy to the context
///    context.Students.Add(std1);
///
///    //save data to the database tables
///    context.SaveChanges();
///    
///    //retrieve all the students from the database
///    foreach (var s in context.Students) {
///        Console.WriteLine($"First Name: {s.FirstName}, Last Name: {s.LastName}");
///    }
///}
/// </code> 
/// Tutorial: <see href="https://www.entityframeworktutorial.net/efcore/working-with-dbcontext.aspx"/>
/// </summary>
public class DatabaseContext : DbContext
{
    public DbSet<HostDevice> HostDevices { get; set; }
    public DbSet<Sector> Sectors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = "database.sqlite3",
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        };
        optionsBuilder.UseSqlite(builder.ToString());
    }
}
