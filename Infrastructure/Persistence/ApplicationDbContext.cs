using Application.Data;
using Domain.Customers;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext ,IUnitOfWork
    {
        //para pubicar eventos de dominio???
        private readonly IPublisher _publisher;

        public  ApplicationDbContext(DbContextOptions options,IPublisher publisher) : base (options)
        {

            _publisher = publisher ?? throw  new  ArgumentNullException(nameof(publisher));
        }
        public DbSet<Customer> Customers {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly); 
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var domainEvents = ChangeTracker.Entries<AggregateRoot>()
                .Select(e=>e.Entity)
                .Where(e => e.GetDomainEvents().Any())
                .SelectMany(e => e.GetDomainEvents());
                
                var result=await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents) 
            { 
                await _publisher.Publish(domainEvent, cancellationToken);

            }
            return result;

                
        }
        
    }
}
