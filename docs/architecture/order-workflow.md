# Order workflow

`Draft → PendingInventory → PendingPayment → Confirmed` is the successful path. Inventory failure or payment failure moves the order to `Cancelled`. Ordering publishes the cancellation, Inventory releases pending reservations, and Notifications records and delivers the final outcome. Each state transition is guarded by the aggregate and driven by an Inbox-protected integration event.
