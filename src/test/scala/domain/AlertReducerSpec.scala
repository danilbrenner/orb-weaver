package domain

import org.scalatest.matchers.should.Matchers
import org.scalatest.wordspec.AnyWordSpec

class AlertReducerSpec extends AnyWordSpec with Matchers {

  case class MockAlertUpdate(
    override val hash: String = "test-hash",
    override val content: String = "test-content",
    evaluationResult: Boolean,
    fieldValues: Map[String, Option[String]] = Map.empty
  ) extends AlertUpdate {
    
    override def getFieldValue(jsonPath: String): Option[String] = 
      fieldValues.getOrElse(jsonPath, None)
    
    override def evaluate(expression: String): Boolean = 
      evaluationResult
  }

  def createEmailNotification(): NotificationConfig.Email =
    NotificationConfig.Email(TextPair("Email: Alert activated", "Email: Alert deactivated"))

  def createTelegramNotification(): NotificationConfig.Telegram =
    NotificationConfig.Telegram(TextPair("Tg: Alert activated", "Tg: Alert deactivated"))

  "AlertReducer.reduce" when {
    "condition evaluates to true and state is Inactive" should {
      "transition to Active state and return activation messages" in {
        val emailNotif = createEmailNotification()
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "service_down",
          keyGetter = "$.service.name",
          condition = "$.status == 'DOWN'",
          notification = List(emailNotif, telegramNotif)
        )
        
        val state = AlertState(
          key = "api-service",
          status = AlertStatus.Inactive
        )
        
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        newState.key shouldBe "api-service"
        actions should have size 2
        actions should contain(NotifyAction.Email("Email: Alert activated"))
        actions should contain(NotifyAction.Telegram("Tg: Alert activated"))
      }

      "work with single email notification" in {
        val emailNotif = createEmailNotification()
        
        val rules = AlertRules(
          name = "high_cpu",
          keyGetter = "$.service",
          condition = "$.cpu > 80",
          notification = List(emailNotif)
        )
        
        val state = AlertState(key = "web-service", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        actions should have size 1
        actions.head shouldBe NotifyAction.Email("Email: Alert activated")
      }

      "work with single telegram notification" in {
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "memory_alert",
          keyGetter = "$.service",
          condition = "$.memory > 90",
          notification = List(telegramNotif)
        )
        
        val state = AlertState(key = "db-service", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        actions should have size 1
        actions.head shouldBe NotifyAction.Telegram("Tg: Alert activated")
      }

      "work with multiple notifications of same type" in {
        val emailNotif1 = createEmailNotification()
        val emailNotif2 = createEmailNotification()
        
        val rules = AlertRules(
          name = "dual_email_alert",
          keyGetter = "$.service",
          condition = "$.error",
          notification = List(emailNotif1, emailNotif2)
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        actions should have size 2
        actions should contain(NotifyAction.Email("Email: Alert activated"))
        actions should contain(NotifyAction.Email("Email: Alert activated"))
      }

      "work with empty notification list" in {
        val rules = AlertRules(
          name = "silent_alert",
          keyGetter = "$.service",
          condition = "$.condition",
          notification = List.empty
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        actions shouldBe empty
      }
    }

    "condition evaluates to false and state is Active" should {
      "transition to Inactive state and return deactivation messages" in {
        val emailNotif = createEmailNotification()
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "service_down",
          keyGetter = "$.service.name",
          condition = "$.status == 'DOWN'",
          notification = List(emailNotif, telegramNotif)
        )
        
        val state = AlertState(
          key = "api-service",
          status = AlertStatus.Active
        )
        
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        newState.key shouldBe "api-service"
        actions should have size 2
        actions should contain(NotifyAction.Email("Email: Alert deactivated"))
        actions should contain(NotifyAction.Telegram("Tg: Alert deactivated"))
      }

      "work with single email notification" in {
        val emailNotif = createEmailNotification()
        
        val rules = AlertRules(
          name = "error_alert",
          keyGetter = "$.service",
          condition = "$.has_error",
          notification = List(emailNotif)
        )
        
        val state = AlertState(key = "app-service", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        actions should have size 1
        actions.head shouldBe NotifyAction.Email("Email: Alert deactivated")
      }

      "work with single telegram notification" in {
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "memory_alert",
          keyGetter = "$.service",
          condition = "$.memory > 90",
          notification = List(telegramNotif)
        )
        
        val state = AlertState(key = "db-service", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        actions should have size 1
        actions.head shouldBe NotifyAction.Telegram("Tg: Alert deactivated")
      }

      "work with multiple notifications of same type" in {
        val emailNotif1 = createEmailNotification()
        val emailNotif2 = createEmailNotification()
        
        val rules = AlertRules(
          name = "dual_email_alert",
          keyGetter = "$.service",
          condition = "$.error",
          notification = List(emailNotif1, emailNotif2)
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        actions should have size 2
        actions should contain(NotifyAction.Email("Email: Alert deactivated"))
        actions should contain(NotifyAction.Email("Email: Alert deactivated"))
      }

      "work with empty notification list" in {
        val rules = AlertRules(
          name = "silent_alert",
          keyGetter = "$.service",
          condition = "$.condition",
          notification = List.empty
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        actions shouldBe empty
      }
    }

    "condition evaluates to true and state is already Active" should {
      "remain in Active state and return no actions" in {
        val emailNotif = createEmailNotification()
        
        val rules = AlertRules(
          name = "test_alert",
          keyGetter = "$.service",
          condition = "$.error",
          notification = List(emailNotif)
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Active
        newState.key shouldBe "service"
        actions shouldBe empty
      }

      "preserve state with multiple notifications configured" in {
        val emailNotif = createEmailNotification()
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "multi_notif_alert",
          keyGetter = "$.key",
          condition = "$.condition",
          notification = List(emailNotif, telegramNotif)
        )
        
        val state = AlertState(key = "test-key", status = AlertStatus.Active)
        val update = MockAlertUpdate(evaluationResult = true)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState shouldBe state
        actions shouldBe empty
      }
    }

    "condition evaluates to false and state is already Inactive" should {
      "remain in Inactive state and return no actions" in {
        val emailNotif = createEmailNotification()
        
        val rules = AlertRules(
          name = "test_alert",
          keyGetter = "$.service",
          condition = "$.error",
          notification = List(emailNotif)
        )
        
        val state = AlertState(key = "service", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState.status shouldBe AlertStatus.Inactive
        newState.key shouldBe "service"
        actions shouldBe empty
      }

      "preserve state with multiple notifications configured" in {
        val emailNotif = createEmailNotification()
        val telegramNotif = createTelegramNotification()
        
        val rules = AlertRules(
          name = "multi_notif_alert",
          keyGetter = "$.key",
          condition = "$.condition",
          notification = List(emailNotif, telegramNotif)
        )
        
        val state = AlertState(key = "test-key", status = AlertStatus.Inactive)
        val update = MockAlertUpdate(evaluationResult = false)
        
        val (newState, actions) = AlertReducer.reduce(rules, state, update)
        
        newState shouldBe state
        actions shouldBe empty
      }
    }
    
    "state transitions form a valid state machine" should {
      "allow Inactive -> Active -> Inactive -> Active cycle" in {
        val emailNotif = createEmailNotification()
        val rules = AlertRules(
          name = "cycling_alert",
          keyGetter = "$.service",
          condition = "$.problem",
          notification = List(emailNotif)
        )
        
        val state1 = AlertState(key = "service", status = AlertStatus.Inactive)
        
        val update1 = MockAlertUpdate(evaluationResult = true)
        val (state2, actions1) = AlertReducer.reduce(rules, state1, update1)
        state2.status shouldBe AlertStatus.Active
        actions1 should have size 1
        
        val update2 = MockAlertUpdate(evaluationResult = false)
        val (state3, actions2) = AlertReducer.reduce(rules, state2, update2)
        state3.status shouldBe AlertStatus.Inactive
        actions2 should have size 1
        
        val update3 = MockAlertUpdate(evaluationResult = true)
        val (state4, actions3) = AlertReducer.reduce(rules, state3, update3)
        state4.status shouldBe AlertStatus.Active
        actions3 should have size 1
      }
    }
  }
}

