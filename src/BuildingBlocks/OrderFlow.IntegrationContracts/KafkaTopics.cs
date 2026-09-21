namespace OrderFlow.IntegrationContracts;

public static class KafkaTopics
{
    public const string CatalogEvents = "orderflow.catalog.events.v1";
    public const string OrderEvents = "orderflow.ordering.events.v1";
    public const string InventoryEvents = "orderflow.inventory.events.v1";
    public const string PaymentEvents = "orderflow.payments.events.v1";
    public const string NotificationEvents = "orderflow.notifications.events.v1";
    public const string DeadLetters = "orderflow.dead-letter.v1";
}
