using Microsoft.EntityFrameworkCore;
using NZWalks.Domain.Entities;

namespace NZWalks.Infrastructure.Data
{
    public class NZWalksDbContext : DbContext
    {
        public NZWalksDbContext(DbContextOptions<NZWalksDbContext> dbContextOptions): base(dbContextOptions)
        {
            
        }

        public DbSet<Region> Regions { get; set; }

        public DbSet<Walk> Walks { get; set; }

        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Walk>()
                .Property(w => w.DifficultyType)
                .HasConversion<string>();
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    //seed data for Difficulties
        //    //Easy, Medium, Hard, Extreme

        //    var difficulties = new List<Difficulty>()
        //    {
        //        new Difficulty()
        //        {
        //            Id = Guid.Parse("0a8dd7ff-4887-4dd2-a689-f0a1cfb829e8"),
        //            Name = "Easy"
        //        },
        //        new Difficulty()
        //        {
        //            Id = Guid.Parse("d1c7d834-19e1-4d1f-9257-390eb3a3d153"),
        //            Name = "Medium"
        //        },
        //        new Difficulty()
        //        {
        //            Id = Guid.Parse("a83ff3aa-b162-4b7d-be66-9cb62c520c7c"),
        //            Name = "Hard"
        //        }
        //    };

        //    //Seed difficulties data to the database
        //    modelBuilder.Entity<Difficulty>().HasData(difficulties);

        //    //Seed data for Regions
        //    var regions = new List<Region>
        //    {
        //        new Region
        //        {
        //            Id = Guid.Parse("f7248fc3-2585-4efb-8d1d-1c555f4087f6"),
        //            Name = "Auckland",
        //            Code = "AKL",
        //            RegionImageUrl = "https://images.pexels.com/photos/5169056/pexels-photo-5169056.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
        //        },
        //        new Region
        //        {
        //            Id = Guid.Parse("6884f7d7-ad1f-4101-8df3-7a6fa7387d81"),
        //            Name = "Northland",
        //            Code = "NTL",
        //            RegionImageUrl = null
        //        },
        //        new Region
        //        {
        //            Id = Guid.Parse("14ceba71-4b51-4777-9b17-46602cf66153"),
        //            Name = "Bay Of Plenty",
        //            Code = "BOP",
        //            RegionImageUrl = null
        //        },
        //        new Region
        //        {
        //            Id = Guid.Parse("cfa06ed2-bf65-4b65-93ed-c9d286ddb0de"),
        //            Name = "Wellington",
        //            Code = "WGN",
        //            RegionImageUrl = "https://images.pexels.com/photos/4350631/pexels-photo-4350631.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
        //        },
        //        new Region
        //        {
        //            Id = Guid.Parse("906cb139-415a-4bbb-a174-1a1faf9fb1f6"),
        //            Name = "Nelson",
        //            Code = "NSN",
        //            RegionImageUrl = "https://images.pexels.com/photos/13918194/pexels-photo-13918194.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
        //        },
        //        new Region
        //        {
        //            Id = Guid.Parse("f077a22e-4248-4bf6-b564-c7cf4e250263"),
        //            Name = "Southland",
        //            Code = "STL",
        //            RegionImageUrl = null
        //        },
        //    };

        //    modelBuilder.Entity<Region>().HasData(regions);
        //}
    }
}
