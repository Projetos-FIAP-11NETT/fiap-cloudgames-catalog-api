using FiapCloudGames.Catalog.Domain.Enums;

namespace FiapCloudGames.Queue.Contracts;


public interface IPaymentProcessed
{
    public int OrderId { get; }
    public decimal Amount { get; }
    public DateTimeOffset PaymentDate { get; }
    public PaymentStatus PaymentStatus { get; }
    public string Email { get; }
    public string Name { get; }
    public string RejectionReason { get; }
}