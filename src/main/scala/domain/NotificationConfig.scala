package domain

sealed trait NotifyAction(text: String)

object NotifyAction {
  final case class Email(text: String) extends NotifyAction(text)

  final case class Telegram(text: String) extends NotifyAction(text)
}

final case class TextPair
(
  onActivate: String,
  onDeactivate: String
)

sealed trait NotificationConfig(text: TextPair) {
  def toActivationMessage: NotifyAction
  def toDeactivationMessage: NotifyAction
}

object NotificationConfig {
  final case class Email
  (text: TextPair) extends NotificationConfig(text) {
    override def toActivationMessage: NotifyAction =
      NotifyAction.Email(text.onActivate)

    override def toDeactivationMessage: NotifyAction =
      NotifyAction.Email(text.onDeactivate)
  }

  final case class Telegram(text: TextPair) extends NotificationConfig(text) {
    override def toActivationMessage: NotifyAction =
      NotifyAction.Telegram(text.onActivate)

    override def toDeactivationMessage: NotifyAction =
      NotifyAction.Telegram(text.onDeactivate)
  }
}

