namespace Domain.Primitives;

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _domainEvent = new();

    public ICollection<DomainEvent> GetDomainEvents() => _domainEvent;

    protected void Raise(DomainEvent domainEvent) {
        _domainEvent.Add(domainEvent);
        
    }

}
