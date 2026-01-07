### New Rules

| Rule ID  | Category | Severity | Notes                                                    |
|----------|----------|----------|----------------------------------------------------------|
| CHSG0001 | Usage    | Error    | SagaStep message type must implement ISagaMessage        |
| CHSG0002 | Usage    | Error    | SagaStepAttribute requires ISaga implementation          |
| CHSG0003 | Usage    | Error    | RetryOnExceptions must contain only Exception types      |
| CHSG0004 | Usage    | Error    | NonRetryableExceptions must contain only Exception types |
| CHSG0005 | Usage    | Error    | Saga can have at most one pivot point                    |
