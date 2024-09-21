using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Models
{
	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
		{
		}

		public DbSet<Users> Users { get; set; }
		public DbSet<Trips> Trips { get; set; }
		public DbSet<Itineraries> Itineraries { get; set; }
		public DbSet<Expenses> Expenses { get; set; }
		public DbSet<Currencies> Currencies { get; set; }
		public DbSet<Notifications> Notifications { get; set; }

		public DbSet<Destinations> Destinations { get; set; }
		public DbSet<Photos> Photos { get; set; }
		public DbSet<Contacts> Contacts { get; set; }
		public DbSet<Admin> Admin { get; set; }

		public DbSet<PhotosImage> PhotosImages { get; set; }
		public DbSet<DestinationImage> DestinationImages { get; set; }
		public DbSet<Tour> Tours { get; set; }
		public DbSet<ExpenseTour> ExpenseTours { get; set; }

		public DbSet<TourImage> TourImages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			//Cấu hinh quan hệ Itineraries
			modelBuilder.Entity<Users>()
				.HasMany(u => u.Trips)
				.WithOne(t => t.User)
				.HasForeignKey(t => t.UserId);

			modelBuilder.Entity<Trips>()
				.HasMany(t => t.Itineraries)
				.WithOne(i => i.Trip)
				.HasForeignKey(i => i.TripId);


			//Cấu hinh quan hệ Itineraries
			modelBuilder.Entity<Itineraries>()
				.HasMany(i => i.Expenses)
				.WithOne(e => e.Itineraries)
				.HasForeignKey(e => e.ItineraryId);

			modelBuilder.Entity<Itineraries>()
			.HasOne(i => i.Trip)
			.WithMany(t => t.Itineraries)
			.HasForeignKey(t => t.TripId);


			modelBuilder.Entity<Destinations>()
				.HasMany(d => d.Photos)
				.WithOne(p => p.Destination)
				.HasForeignKey(p => p.DestinationId);

			modelBuilder.Entity<Users>()
				.HasMany(u => u.Notifications)
				.WithOne(n => n.User)
				.HasForeignKey(n => n.UserId);

			modelBuilder.Entity<Users>()
				.HasIndex(t => t.Email)
				.IsUnique();

			modelBuilder.Entity<Users>()
				.HasIndex(t => t.Phone)
				.IsUnique();
			modelBuilder.Entity<Destinations>()
			  .HasMany(p => p.DestinationImages)
			  .WithOne(p => p.Destinations)
			  .HasForeignKey(p => p.DestinationId);

			modelBuilder.Entity<Photos>()
				.HasMany(p => p.PhotosImages)
				.WithOne(p => p.Photos)
				.HasForeignKey(p => p.PhotoId);

			modelBuilder.Entity<Tour>()
				 .HasMany(p => p.ExpenseTours)
				 .WithOne(p => p.Tours)
				 .HasForeignKey(p => p.TourID);

			modelBuilder.Entity<ExpenseTour>()
				.HasMany(p => p.TourImages)
				.WithOne(p => p.ExpenseTours)
				.HasForeignKey(p => p.ExpenseTourId);
		}
	}
}

