using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using ZPIServer.Models;

namespace ZPIServer.Commands;

public class DbCommand : Command
{
    public const string ListAllArgument = "listall";
    public const string TestArgument = "test"; //not listed in help messages

    public string? FirstArg { get; private set; }
    public string? SecondArg { get; private set; }

    public DbCommand(Logger? logger = null) : base(logger)
    {
    }

    public override void Execute()
    {
        switch (FirstArg)
        {
            case null:
                _logger?.WriteLine($"{Command.Db} requires 1 or more arguments.");
                _logger?.WriteLine(GetHelp());
                break;
            case ListAllArgument:
                try
                {
                    using var context = new DatabaseContext();
                    int recordCount = 0;
                    _logger?.WriteLine($"Records in {nameof(DatabaseContext.HostDevices)}:");
                    foreach (var record in context.HostDevices.ToList())
                    {
                        _logger?.WriteLine(record.ToString());
                        recordCount++;
                    }
                    _logger?.WriteLine($"\tTotal amount: {recordCount}");

                    recordCount = 0;
                    _logger?.WriteLine($"Records in {nameof(DatabaseContext.Sectors)}:");
                    foreach (var record in context.Sectors.ToList())
                    {
                        _logger?.WriteLine(record.ToString());
                        recordCount++;
                    }
                    _logger?.WriteLine($"\tTotal amount: {recordCount}");
                }
                catch (SqliteException ex)
                {
                    _logger?.WriteLine($"ERROR! Something wrong with the database file: {ex.Message}");
                }
                break;
            case TestArgument:
                //Not listed in help page - For CRUD testing
                //Do NOT run command with this arg when server has an actual database - it WILL be erased
                switch (SecondArg)
                {
                    case "c": //Create
                        try
                        {
                            using var context = new DatabaseContext();
                            context.Database.EnsureDeleted();
                            context.Database.Migrate();
                            Sector sectorA = new()
                            {
                                Name = "Sektor A",
                                LastStatus = Sector.FireStatus.OK,
                                Description = "Przykładowy opis 123"
                            };
                            Sector sectorB = new()
                            {
                                Name = "Sektor B",
                                LastStatus = Sector.FireStatus.Suspected,
                                Description = "Przykładowy opis 123"
                            };
                            Sector sectorC = new()
                            {
                                Name = "Sektor C",
                                LastStatus = Sector.FireStatus.Confirmed,
                                Description = "Zgłosiła Jadwiga Hymel"
                            };
                            HostDevice camera1 = new()
                            {
                                Name = "Kamera 1 - Optrix Model 123A",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("1.2.3.4"),
                                Sector = sectorB,
                                LastKnownStatus = HostDevice.DeviceStatus.OK,
                                LastTemperature = (decimal?)24.3,
                                ExactLocation = "123N,321W"
                            };
                            HostDevice camera2 = new()
                            {
                                Name = "Kamera 2 - Optrix Model 123A",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("1.2.3.5"),
                                Sector = sectorA,
                                LastKnownStatus = HostDevice.DeviceStatus.OK | HostDevice.DeviceStatus.LowPower,
                                LastTemperature = (decimal?)5.2,
                                ExactLocation = "125N,326W"
                            };
                            HostDevice camera3 = new()
                            {
                                Name = "Kamera 3 - Optrix Model 123B",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("1.2.3.6"),
                                Sector = sectorC,
                                LastKnownStatus = HostDevice.DeviceStatus.Unresponsive,
                                LastTemperature = (decimal?)1526.2,
                                ExactLocation = "121N,321W"
                            };
                            HostDevice user1 = new()
                            {
                                Name = "Operator Nadleśnictwa",
                                Type = HostType.User,
                                Address = IPAddress.Parse("1.2.3.2")
                            };
                            HostDevice user2 = new()
                            {
                                Name = "Operator KWPSP",
                                Type = HostType.User,
                                Address = IPAddress.Parse("1.2.3.3")
                            };
                            context.Sectors.Add(sectorA);
                            context.Sectors.Add(sectorB);
                            context.Sectors.Add(sectorC);
                            context.HostDevices.Add(camera1);
                            context.HostDevices.Add(camera2);
                            context.HostDevices.Add(camera3);
                            context.HostDevices.Add(user1);
                            context.HostDevices.Add(user2);
                            context.SaveChanges();
                            _logger?.WriteLine("Test for Create performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}");
                        }
                        break;
                    case "r": //Read
                        try
                        {
                            using var context = new DatabaseContext();

                            IList<HostDevice> devices = context.HostDevices.ToList();
                            _logger?.WriteLine("Test for normal devices - Queried all HostDevices:");
                            foreach (var device in devices)
                                _logger?.WriteLine(device.ToString());

                            IList<Sector> sectors = context.Sectors.ToList();
                            _logger?.WriteLine("Test for normal devices - Queried all Sectors:");
                            foreach (var device in sectors)
                                _logger?.WriteLine(device.ToString());

                            _logger?.WriteLine("Test for relational mapping - Query all related Sectors:");
                            foreach (var device in devices) 
                                _logger?.WriteLine(device.ToString() + "\n" + device.Sector?.ToString());

                            _logger?.WriteLine("Test for querying by ID = 1:");
                            _logger?.WriteLine(context.HostDevices.Find((HostDevice x) => x.Id == 1)?.ToString());
                            _logger?.WriteLine(context.Sectors.Find((Sector x) => x.Id == 1)?.ToString());

                            _logger?.WriteLine("Test for Read performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}");
                        }
                        break;
                    case "u": //Update
                        try
                        {
                            using var context = new DatabaseContext();

                            _logger?.WriteLine("Queried all HostDevices:");
                            foreach (var device in context.HostDevices.ToList())
                                _logger?.WriteLine(device.ToString());

                            _logger?.WriteLine("Applying standard change.");
                            var hostDevice = context.HostDevices.Find((HostDevice x) => x.Id == 1);
                            hostDevice.LastTemperature = (decimal?)1234.56;
                            hostDevice.Address = IPAddress.Parse("123.123.123.123");
                            context.SaveChanges();

                            _logger?.WriteLine("Applying relation change.");
                            var anotherSector = context.Sectors.Find((Sector s) => s.Id != hostDevice.SectorId );
                            hostDevice.Sector = anotherSector;
                            context.SaveChanges();

                            _logger?.WriteLine("Queried all HostDevices:");
                            foreach (var device in context.HostDevices.ToList())
                                _logger?.WriteLine(device.ToString());

                            _logger?.WriteLine("Test for Update performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}");
                        }
                        break;
                    case "d": //Delete
                        try
                        {
                            using var context = new DatabaseContext();

                            _logger?.WriteLine("Queried all HostDevices and Sectors:");
                            foreach (var device in context.HostDevices.ToList())
                                _logger?.WriteLine(device.ToString());
                            foreach (var sector in context.Sectors.ToList())
                                _logger?.WriteLine(sector.ToString());

                            _logger?.WriteLine("Standard removal.");
                            var hostDevice = context.HostDevices.Find((HostDevice x) => x.Id == 3);
                            context.HostDevices.Remove(hostDevice);
                            context.SaveChanges();

                            _logger?.WriteLine("Related object removal.");
                            var sectorToRemove = context.Sectors.Find((Sector s) => s.Id != hostDevice.SectorId);
                            context.Sectors.Remove(sectorToRemove);
                            context.SaveChanges();

                            _logger?.WriteLine("Queried all HostDevices and Sectors:");
                            foreach (var device in context.HostDevices.ToList())
                                _logger?.WriteLine(device.ToString());
                            foreach (var sector in context.Sectors.ToList())
                                _logger?.WriteLine(sector.ToString());

                            _logger?.WriteLine("Test for Delete performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}");
                        }
                        break;
                    default:
                        _logger?.WriteLine("No test performed - invalid second arg (c/r/u/d).");
                        break;
                }
                break;
            default:
                _logger?.WriteLine("Unrecognized argument.");
                _logger?.WriteLine(GetHelp());
                break;
        }
        Invoke(this, new EventArgs.CommandEventArgs());
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Command for interacting with the server's database. Available arguments:");
        builder.AppendLine($"\t- {ListAllArgument}");
        builder.AppendLine("\tShows all records from all tables.");

        builder.AppendLine("Examples:");
        builder.AppendLine($"\t{Command.Db} {ListAllArgument}");
        return builder.ToString();
    }

    public override void SetArguments(params string[]? arguments)
    {
        if (arguments is null || arguments.Length == 0)
            return;

        if (arguments.Length == 1)
        {
            FirstArg = arguments[0];
        }
        else if (arguments.Length == 2)
        {
            FirstArg = arguments[0];
            SecondArg = arguments[1];
        }
        else
        {
            _logger?.WriteLine("Too many arguments.");
        }
    }
}
