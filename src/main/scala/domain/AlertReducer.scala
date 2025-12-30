package domain

object AlertReducer {
  def reduce(rules: AlertRules, state: AlertState, update: AlertUpdate): (AlertState, List[NotifyAction]) =
    (update.evaluate(rules.condition), state.status) match {
      case (true, AlertStatus.Inactive) =>
        (state.copy(status = AlertStatus.Active), rules.notification.map(_.toActivationMessage))
      case (false, AlertStatus.Active) =>
        (state.copy(status = AlertStatus.Inactive), rules.notification.map(_.toDeactivationMessage))
      case _ =>
        (state, List())
    }
}