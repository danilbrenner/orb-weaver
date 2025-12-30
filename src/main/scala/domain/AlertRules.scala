package domain

final case class AlertRules
(
  name: String,
  keyGetter: String,
  condition: String,
  notification: List[NotificationConfig]
)