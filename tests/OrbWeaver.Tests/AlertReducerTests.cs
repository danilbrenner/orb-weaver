using System.Collections.Immutable;
using OrbWeaver.Domain;
using Moq;

namespace OrbWeaver.Tests;

public class AlertReducerTests
{
    private static Alert CreateTestAlert(
        AlertStatus status = AlertStatus.Resolved,
        string name = "Test Alert",
        string condition = "$.error")
    {
        return new Alert(
            Id: Guid.NewGuid(),
            Name: name,
            Condition: condition,
            Status: status,
            Timestamp: DateTime.UtcNow
        );
    }

    [Fact]
    public void Reduce_ConditionTrue_StatusResolved_ActivatesAlert()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Resolved);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(true);

        // Act
        var (newAlert, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Equal(AlertStatus.Active, newAlert.Status);
        Assert.Single(notifications);
        Assert.Equal(alert.Name, notifications[0].AlertName);
        Assert.Equal(AlertStatus.Active, notifications[0].NewStatus);
    }

    [Fact]
    public void Reduce_ConditionFalse_StatusActive_ResolvesAlert()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Active);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(false);

        // Act
        var (newAlert, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Equal(AlertStatus.Resolved, newAlert.Status);
        Assert.Single(notifications);
        Assert.Equal(alert.Name, notifications[0].AlertName);
        Assert.Equal(AlertStatus.Resolved, notifications[0].NewStatus);
    }

    [Fact]
    public void Reduce_ConditionTrue_StatusActive_NoChange()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Active);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(true);

        // Act
        var (newAlert, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Equal(AlertStatus.Active, newAlert.Status);
        Assert.Equal(alert, newAlert);
        Assert.Empty(notifications);
    }

    [Fact]
    public void Reduce_ConditionFalse_StatusResolved_NoChange()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Resolved);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(false);

        // Act
        var (newAlert, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Equal(AlertStatus.Resolved, newAlert.Status);
        Assert.Equal(alert, newAlert);
        Assert.Empty(notifications);
    }

    [Fact]
    public void Reduce_PreservesAlertProperties_ExceptStatus()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var alertName = "Critical Alert";
        var condition = "$.critical.error";
        var timestamp = DateTime.UtcNow;
        
        var alert = new Alert(
            Id: alertId,
            Name: alertName,
            Condition: condition,
            Status: AlertStatus.Resolved,
            Timestamp: timestamp
        );
        
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(condition)).Returns(true);

        // Act
        var (newAlert, _) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Equal(alertId, newAlert.Id);
        Assert.Equal(alertName, newAlert.Name);
        Assert.Equal(condition, newAlert.Condition);
        Assert.Equal(timestamp, newAlert.Timestamp);
        Assert.Equal(AlertStatus.Active, newAlert.Status); // Only status changed
    }

    [Fact]
    public void Reduce_NotificationContainsCorrectAlertName()
    {
        // Arrange
        var alertName = "Custom Alert Name";
        var alert = CreateTestAlert(status: AlertStatus.Resolved, name: alertName);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(true);

        // Act
        var (_, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.Single(notifications);
        Assert.Equal(alertName, notifications[0].AlertName);
    }

    [Fact]
    public void Reduce_EvaluatesCorrectCondition()
    {
        // Arrange
        var condition = "$.custom.path[?(@.level == 'error')]";
        var alert = CreateTestAlert(status: AlertStatus.Resolved, condition: condition);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(condition)).Returns(true);

        // Act
        AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        mockUpdate.Verify(u => u.Evaluate(condition), Times.Once);
    }

    [Fact]
    public void Reduce_MultipleStateTransitions()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Resolved);
        var mockUpdate1 = new Mock<UpdateMessage>();
        mockUpdate1.Setup(u => u.Evaluate(alert.Condition)).Returns(true);
        
        var mockUpdate2 = new Mock<UpdateMessage>();
        mockUpdate2.Setup(u => u.Evaluate(alert.Condition)).Returns(false);

        // Act - First transition: Resolved -> Active
        var (intermediateAlert, notifications1) = AlertReducer.Reduce(alert, mockUpdate1.Object);
        
        // Act - Second transition: Active -> Resolved
        var (finalAlert, notifications2) = AlertReducer.Reduce(intermediateAlert, mockUpdate2.Object);

        // Assert
        Assert.Equal(AlertStatus.Active, intermediateAlert.Status);
        Assert.Single(notifications1);
        Assert.Equal(AlertStatus.Active, notifications1[0].NewStatus);
        
        Assert.Equal(AlertStatus.Resolved, finalAlert.Status);
        Assert.Single(notifications2);
        Assert.Equal(AlertStatus.Resolved, notifications2[0].NewStatus);
    }

    [Fact]
    public void Reduce_ReturnsImmutableNotificationList()
    {
        // Arrange
        var alert = CreateTestAlert(status: AlertStatus.Resolved);
        var mockUpdate = new Mock<UpdateMessage>();
        mockUpdate.Setup(u => u.Evaluate(alert.Condition)).Returns(true);

        // Act
        var (_, notifications) = AlertReducer.Reduce(alert, mockUpdate.Object);

        // Assert
        Assert.IsType<ImmutableList<Notification>>(notifications);
    }
}

