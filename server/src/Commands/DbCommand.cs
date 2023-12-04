using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using ZPICommunicationModels.Models;
using static ZPICommunicationModels.Models.HostDevice;

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
                _logger?.WriteLine($"{Db} requires 1 or more arguments.");
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
                    _logger?.WriteLine($"ERROR! Something went wrong with the database file: {ex.Message}", messageType: Logger.MessageType.Error);
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
                                Description = "Przykładowy opis 123"
                            };
                            Sector sectorB = new()
                            {
                                Name = "Sektor B",
                                Description = "Przykładowy opis 123"
                            };
                            Sector sectorC = new()
                            {
                                Name = "Sektor C",
                                Description = "Zgłosiła Jadwiga Hymel"
                            };
                            HostDevice camera1 = new()
                            {
                                Name = "Kamera 1 - Optrix Model 123A",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("127.0.0.1"),
                                Port = 12000,
                                Sector = sectorB,
                                LastDeviceStatus = DeviceStatus.OK,
                                LastFireStatus = FireStatus.Confirmed,
                                LastKnownTemperature = 24.3m,
                                LocationAltitude = 12.3456789010m,
                                LocationLatitude = 23.192488583m
                            };
                            HostDevice camera2 = new()
                            {
                                Name = "Kamera 2 - Optrix Model 123A",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("1.2.3.5"),
                                Port = 12000,
                                Sector = sectorA,
                                LastDeviceStatus = DeviceStatus.LowPower,
                                LastFireStatus = FireStatus.Suspected,
                                LastKnownTemperature = 5.2m,
                                LocationAltitude = 12.235687879543m,
                                LocationLatitude = 23.19292929292m
                            };
                            HostDevice camera3 = new()
                            {
                                Name = "Kamera 3 - Optrix Model 123B",
                                Type = HostType.CameraSimulator,
                                Address = IPAddress.Parse("1.2.3.6"),
                                Port = 12000,
                                Sector = sectorC,
                                LastDeviceStatus = DeviceStatus.Unresponsive,
                                LastFireStatus = FireStatus.OK,
                                LastKnownTemperature = 1526.2m,
                                LocationAltitude = 12.2345646646666m,
                                LocationLatitude = 23.1234444111234m
                            };
                            HostDevice user1 = new()
                            {
                                Name = "Operator Nadleśnictwa",
                                Type = HostType.User,
                                Address = IPAddress.Parse("1.2.3.2"),
                                Port = 12000
                            };
                            HostDevice user2 = new()
                            {
                                Name = "Operator KWPSP",
                                Type = HostType.User,
                                Address = IPAddress.Parse("1.2.3.3"),
                                Port = 12000
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
                            _logger?.WriteLine($"ERROR! {ex.Message}", messageType: Logger.MessageType.Error);
                        }
                        break;
                    case "r": //Read
                        try
                        {
                            using var context = new DatabaseContext();

                            IList<HostDevice> devices = context.HostDevices.Include(nameof(HostDevice.Sector)).ToList();
                            _logger?.WriteLine("Test for normal devices - Queried all HostDevices:");
                            foreach (var device in devices)
                                _logger?.WriteLine(device.ToString());

                            IList<Sector> sectors = context.Sectors.ToList();
                            _logger?.WriteLine("Test for normal devices - Queried all Sectors:");
                            foreach (var sector in sectors)
                                _logger?.WriteLine(sector.ToString());

                            _logger?.WriteLine("Test for relational mapping - Query all related Sectors:");
                            foreach (var device in devices)
                                _logger?.WriteLine(device.Sector?.ToString());

                            _logger?.WriteLine("Test for querying by ID = 1:");
                            _logger?.WriteLine(context.HostDevices.Where((HostDevice x) => x.Id == 1)?.First().ToString());
                            _logger?.WriteLine(context.Sectors.Where((Sector x) => x.Id == 1)?.First().ToString());

                            _logger?.WriteLine("Test for Read performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}", messageType: Logger.MessageType.Error);
                        }
                        break;
                    case "u": //Update
                        try
                        {
                            using var context = new DatabaseContext();

                            _logger?.WriteLine("Queried all HostDevices:");
                            foreach (var device in context.HostDevices.Include(nameof(HostDevice.Sector)).ToList())
                                _logger?.WriteLine(device.ToString());

                            _logger?.WriteLine("Applying standard change.");
                            var hostDevice = context.HostDevices.Where((HostDevice x) => x.Type != HostType.User).First();
                            hostDevice.LastKnownTemperature = 1234.56m;
                            hostDevice.Address = IPAddress.Parse("123.123.123.123");
                            context.SaveChanges();

                            _logger?.WriteLine("Applying relation change.");
                            var anotherSector = context.Sectors.Where((Sector s) => s.Id != hostDevice.SectorId).First();
                            hostDevice.Sector = anotherSector;
                            context.SaveChanges();

                            _logger?.WriteLine("Queried all HostDevices:");
                            foreach (var device in context.HostDevices.Include(nameof(HostDevice.Sector)).ToList())
                                _logger?.WriteLine(device.ToString());

                            _logger?.WriteLine("Test for Update performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}", messageType: Logger.MessageType.Error);
                        }
                        break;
                    case "d": //Delete
                        try
                        {
                            using var context = new DatabaseContext();

                            _logger?.WriteLine("Queried all HostDevices and Sectors:");
                            foreach (var device in context.HostDevices.Include(nameof(HostDevice.Sector)).ToList())
                                _logger?.WriteLine(device.ToString());
                            foreach (var sector in context.Sectors.ToList())
                                _logger?.WriteLine(sector.ToString());

                            _logger?.WriteLine("Standard removal.");
                            var hostDevice = context.HostDevices.Where((HostDevice x) => x.Id == 3).First();
                            context.HostDevices.Remove(hostDevice);
                            context.SaveChanges();

                            _logger?.WriteLine("Related object removal.");
                            var sectorToRemove = context.Sectors.Where((Sector s) => s.Id != hostDevice.SectorId).First();
                            context.Sectors.Remove(sectorToRemove);
                            context.SaveChanges();

                            _logger?.WriteLine("Queried all HostDevices and Sectors:");
                            foreach (var device in context.HostDevices.Include(nameof(HostDevice.Sector)).ToList())
                                _logger?.WriteLine(device.ToString());
                            foreach (var sector in context.Sectors.ToList())
                                _logger?.WriteLine(sector.ToString());

                            _logger?.WriteLine("Test for Delete performed.");
                        }
                        catch (SqliteException ex)
                        {
                            _logger?.WriteLine($"ERROR! {ex.Message}", messageType: Logger.MessageType.Error);
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
        Invoke(this, System.EventArgs.Empty);
    }

    public override string GetHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Command for interacting with the server's database. Available arguments:");
        builder.AppendLine($"\t- {ListAllArgument}");
        builder.AppendLine("\tShows all records from all tables.");

        builder.AppendLine("Examples:");
        builder.AppendLine($"\t{Db} {ListAllArgument}");
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
