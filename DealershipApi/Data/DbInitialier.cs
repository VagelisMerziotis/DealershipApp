namespace DealershipApi.Data;
using Models;

public class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        /*
        Seeding takes place when the program initializes
        Check if Dealerships table has entries - if not then proceed to seeding
        */
        if (context.Dealerships.Any())
        {
            return;
        }

        // Create and add Dealerships
        var athensDealership = new Dealership
        {
            Id = 1,
            Name = "Athens Auto Moto",
            Address = "Kifisias",
            Number = "214",
            Zipcode = 15231,
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Phone = "2130039210",
            Email = "kifisias@automoto.gr",
            Website = "https://www.automoto.gr",
            Budget = Math.Round(250000m, 2),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        };

        var patraDealership = new Dealership
        {
            Id = 2,
            Name = "Patra Auto Moto",
            Address = "Korinthou",
            Number = "380",
            Zipcode = 22622,
            City = "Patra",
            State = "Axaia",
            Country = "Greece",
            Phone = "2616003923",
            Email = "patra@automoto.gr",
            Website = null,
            Budget = Math.Round(300000m, 2),
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        };

        context.Dealerships.Add(athensDealership);
        context.Dealerships.Add(patraDealership);

        context.Users.Add(new User
        {
            Id = 1,
            Dealership = athensDealership,
            Role = "Retail",
            Username = "Jenny",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Jenny1967!"),
            Email = "jenny.gk@automoto.gr",
            FirstName = "Jenny",
            LastName = "Gkoria",
            Address = "Kalamou 34",
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Salary = 1232,
            IsStakeholder = false
        });

        context.Users.Add(new User
        {
            Id = 2,
            Role = "Sales",
            Dealership = athensDealership,
            Username = "Nikos",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Nikos_2024!"),
            Email = "nikos.pap@automoto.gr",
            FirstName = "Nikos",
            LastName = "Papadopoulos",
            Address = "Ermou 122",
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Salary = 1350,
            IsStakeholder = false
        });

        context.Users.Add(new User
        {
            Id = 3,
            Role = "Manager",
            Dealership = athensDealership,
            Username = "Elena",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Elena!Manage88"),
            Email = "elena.k@automoto.gr",
            FirstName = "Elena",
            LastName = "Konstantinou",
            Address = "Patission 200",
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Salary = 2100,
            IsStakeholder = true
        });

        context.Users.Add(new User
        {
            Id = 4,
            Role = "Mechanic",
            Dealership = patraDealership,
            Username = "Yorgos",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Yorgos_Fix99"),
            Email = "yorgos.m@automoto.gr",
            FirstName = "Yorgos",
            LastName = "Michailidis",
            Address = "Egnatia 45",
            City = "Patra",
            State = "Axaia",
            Country = "Greece",
            Salary = 1180,
            IsStakeholder = false
        });

        context.Users.Add(new User
        {
            Id = 5,
            Role = "Admin",
            Dealership = athensDealership,
            Username = "Sophia",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sophia#Admin01"),
            Email = "sophia.admin@automoto.gr",
            FirstName = "Sophia",
            LastName = "Anagnostou",
            Address = "Panepistimiou 5",
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Salary = 2500,
            IsStakeholder = true
        });

        context.Users.Add(new User
        {
            Id = 6,
            Role = "Retail",
            Dealership = patraDealership,
            Username = "Dimitris",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dimitris_777"),
            Email = "dimitris.v@automoto.gr",
            FirstName = "Dimitris",
            LastName = "Vasileiou",
            Address = "Tsimiski 88",
            City = "Patra",
            State = "Axaia",
            Country = "Greece",
            Salary = 1290,
            IsStakeholder = false
        });

        context.Users.Add(new User
        {
            Id = 7,
            Role = "Sales",
            Dealership = athensDealership,
            Username = "Katerina",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Katerina$Sales22"),
            Email = "katerina.f@automoto.gr",
            FirstName = "Katerina",
            LastName = "Floropoulou",
            Address = "Kifisias 150",
            City = "Athens",
            State = "Attiki",
            Country = "Greece",
            Salary = 1400,
            IsStakeholder = false
        });

        List<Automobile> Cars = new List<Automobile> { };

        Cars.Add(new Automobile
        {
            Id = 1,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Opel",
            Model = "Azda",
            Color = "Red",
            Description = null,
            Type = "Hatchback",
            YearOfManufacture = 1996,
            Price = 16000m,
            EngineVolume = 1400m,
            Used = true,
            Created = DateTime.UtcNow - new TimeSpan(3623),
            Modified = DateTime.UtcNow - new TimeSpan(1502),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 1600m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 2,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Toyota",
            Model = "Corolla",
            Color = "White",
            Description = "Reliable daily driver, single owner.",
            Type = "Sedan",
            YearOfManufacture = 2015,
            Price = 13500m,
            EngineVolume = 1600m,
            Used = true,
            Created = DateTime.UtcNow - new TimeSpan(1505),
            Modified = DateTime.UtcNow - new TimeSpan(472),
            SideOfSteering = "left",
            DoorsNumber = 4,
            HasStorage = true,
            StorageSize = 470m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 3,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Volkswagen",
            Model = "Golf",
            Color = "Blue",
            Description = null,
            Type = "Hatchback",
            YearOfManufacture = 2018,
            Price = 17800m,
            EngineVolume = 1400m,
            Used = true,
            Created = new DateTime(2018, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 4, 2, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 380m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 4,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Ford",
            Model = "Fiesta",
            Color = "Black",
            Description = "Compact city car, low mileage.",
            Type = "Hatchback",
            YearOfManufacture = 2019,
            Price = 12900m,
            EngineVolume = 1100m,
            Used = true,
            Created = new DateTime(2019, 5, 14, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 8, 30, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 3,
            HasStorage = true,
            StorageSize = 292m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 5,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "BMW",
            Model = "320i",
            Color = "Grey",
            Description = "Sport package, well maintained.",
            Type = "Sedan",
            YearOfManufacture = 2020,
            Price = 32000m,
            EngineVolume = 2000m,
            Used = true,
            Created = new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 4,
            HasStorage = true,
            StorageSize = 480m,
            HasCrashedOnce = false,
            Gears = 8
        });

        Cars.Add(new Automobile
        {
            Id = 6,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Mercedes-Benz",
            Model = "C200",
            Color = "Silver",
            Description = null,
            Type = "Sedan",
            YearOfManufacture = 2021,
            Price = 38500m,
            EngineVolume = 2000m,
            Used = false,
            Created = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 4,
            HasStorage = true,
            StorageSize = 455m,
            HasCrashedOnce = false,
            Gears = 9
        });

        Cars.Add(new Automobile
        {
            Id = 7,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Honda",
            Model = "Civic",
            Color = "Red",
            Description = "Sporty trim, aftermarket exhaust.",
            Type = "Hatchback",
            YearOfManufacture = 2017,
            Price = 15200m,
            EngineVolume = 1500m,
            Used = true,
            Created = new DateTime(2017, 11, 3, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2022, 6, 18, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 420m,
            HasCrashedOnce = true,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 8,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Fiat",
            Model = "500",
            Color = "Yellow",
            Description = "Fun city runabout.",
            Type = "Hatchback",
            YearOfManufacture = 2016,
            Price = 9800m,
            EngineVolume = 900m,
            Used = true,
            Created = new DateTime(2016, 4, 22, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2021, 3, 9, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 3,
            HasStorage = true,
            StorageSize = 185m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 9,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Audi",
            Model = "A4",
            Color = "Black",
            Description = null,
            Type = "Sedan",
            YearOfManufacture = 2019,
            Price = 27500m,
            EngineVolume = 2000m,
            Used = true,
            Created = new DateTime(2019, 10, 8, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 4,
            HasStorage = true,
            StorageSize = 480m,
            HasCrashedOnce = false,
            Gears = 7
        });

        Cars.Add(new Automobile
        {
            Id = 10,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Nissan",
            Model = "Qashqai",
            Color = "White",
            Description = "Family SUV, panoramic roof.",
            Type = "SUV",
            YearOfManufacture = 2020,
            Price = 22300m,
            EngineVolume = 1300m,
            Used = true,
            Created = new DateTime(2020, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 430m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 11,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Peugeot",
            Model = "208",
            Color = "Blue",
            Description = null,
            Type = "Hatchback",
            YearOfManufacture = 2022,
            Price = 16700m,
            EngineVolume = 1200m,
            Used = false,
            Created = new DateTime(2022, 3, 4, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2022, 3, 4, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 311m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 12,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Skoda",
            Model = "Octavia",
            Color = "Grey",
            Description = "Spacious estate, diesel.",
            Type = "Estate",
            YearOfManufacture = 2018,
            Price = 18900m,
            EngineVolume = 1600m,
            Used = true,
            Created = new DateTime(2018, 8, 17, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 5, 11, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 610m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 13,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Renault",
            Model = "Clio",
            Color = "Orange",
            Description = null,
            Type = "Hatchback",
            YearOfManufacture = 2014,
            Price = 8200m,
            EngineVolume = 1200m,
            Used = true,
            Created = new DateTime(2014, 1, 25, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2020, 7, 14, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 300m,
            HasCrashedOnce = true,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 14,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Hyundai",
            Model = "Tucson",
            Color = "Green",
            Description = "Hybrid variant, low emissions.",
            Type = "SUV",
            YearOfManufacture = 2023,
            Price = 29900m,
            EngineVolume = 1600m,
            Used = false,
            Created = new DateTime(2023, 1, 9, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 1, 9, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 546m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 15,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Mazda",
            Model = "CX-5",
            Color = "Red",
            Description = null,
            Type = "SUV",
            YearOfManufacture = 2019,
            Price = 24500m,
            EngineVolume = 2200m,
            Used = true,
            Created = new DateTime(2019, 4, 2, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 9, 27, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 442m,
            HasCrashedOnce = false,
            Gears = 6
        });

        Cars.Add(new Automobile
        {
            Id = 16,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Opel",
            Model = "Corsa",
            Color = "White",
            Description = "Entry-level trim, economical.",
            Type = "Hatchback",
            YearOfManufacture = 2017,
            Price = 10400m,
            EngineVolume = 1000m,
            Used = true,
            Created = new DateTime(2017, 6, 6, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2021, 11, 19, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 285m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 17,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Citroën",
            Model = "C3",
            Color = "Blue",
            Description = null,
            Type = "Hatchback",
            YearOfManufacture = 2015,
            Price = 8900m,
            EngineVolume = 1200m,
            Used = true,
            Created = new DateTime(2015, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2019, 12, 5, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 300m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 18,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Volvo",
            Model = "XC60",
            Color = "Black",
            Description = "Premium SUV, full leather interior.",
            Type = "SUV",
            YearOfManufacture = 2022,
            Price = 41000m,
            EngineVolume = 2000m,
            Used = false,
            Created = new DateTime(2022, 10, 21, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2022, 10, 21, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 483m,
            HasCrashedOnce = false,
            Gears = 8
        });

        Cars.Add(new Automobile
        {
            Id = 19,
            DealershipId = 1,
            Dealership = athensDealership,
            Brand = "Kia",
            Model = "Sportage",
            Color = "Grey",
            Description = null,
            Type = "SUV",
            YearOfManufacture = 2020,
            Price = 21800m,
            EngineVolume = 1600m,
            Used = true,
            Created = new DateTime(2020, 5, 16, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2024, 3, 8, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 503m,
            HasCrashedOnce = false,
            Gears = 7
        });

        Cars.Add(new Automobile
        {
            Id = 20,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Seat",
            Model = "Ibiza",
            Color = "Yellow",
            Description = "Youthful hatchback, low tax bracket.",
            Type = "Hatchback",
            YearOfManufacture = 2021,
            Price = 14300m,
            EngineVolume = 1000m,
            Used = true,
            Created = new DateTime(2021, 2, 27, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2023, 10, 3, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 355m,
            HasCrashedOnce = false,
            Gears = 5
        });

        Cars.Add(new Automobile
        {
            Id = 21,
            DealershipId = 2,
            Dealership = patraDealership,
            Brand = "Alfa Romeo",
            Model = "Giulietta",
            Color = "Red",
            Description = "Sport trim, manual transmission.",
            Type = "Hatchback",
            YearOfManufacture = 2016,
            Price = 13100m,
            EngineVolume = 1750m,
            Used = true,
            Created = new DateTime(2016, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2022, 4, 25, 0, 0, 0, DateTimeKind.Utc),
            SideOfSteering = "left",
            DoorsNumber = 5,
            HasStorage = true,
            StorageSize = 350m,
            HasCrashedOnce = true,
            Gears = 6
        });

        // Add the list of cars in the Vehicles Table
        context.Vehicles.AddRange(Cars);

        // Commit the changes to the tables
        context.SaveChanges();
    }
}