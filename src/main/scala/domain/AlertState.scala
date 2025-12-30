package domain

sealed trait AlertStatus

object AlertStatus {
  case object Active extends AlertStatus

  case object Inactive extends AlertStatus
}

final case class AlertState
(
  key: String,
  status: AlertStatus
)

