# Event Broker for Unity

## Table of Contents
- [Introduction](#introduction)
- [Purpose](#purpose)
- [Key Features](#key-features)
  - [Subscription](#subscription)
  - [Unsubscription](#unsubscription)
  - [Call Types](#call-types)
  - [Data Lifetime Management](#data-lifetime-management)
  - [Generic and Standard Data Types](#generic-and-standard-data-types)
  - [Using Tags](#using-tags)
- [Usage Examples](#usage-examples)
- [Installation](#installation)
- [Contributing](#contributing)
- [License](#license)

## Introduction
Event Broker is a lightweight event handling framework designed for Unity. It simplifies the communication between different components of your game by enabling a publish-subscribe model, allowing for clean and decoupled code.

## Purpose
The primary goal of this framework is to provide a robust and flexible event management system that enhances the scalability and maintainability of Unity projects. By separating event producers from consumers, developers can avoid tight coupling and improve code organization.

## Key Features

### Subscription
The Event Broker allows components to subscribe to specific events, enabling them to react to those events when published.

#### Example:
```csharp
EventBroker.Instance.Subscribe("PlayerScored", OnPlayerScored);

private void OnPlayerScored(EventArgs args)
{
    UpdateScoreUI((int)args.Message);
}
```

### Unsubscription
Components can unsubscribe from events to stop receiving notifications. This is useful for managing event listeners and preventing memory leaks.

#### Example:
```csharp
EventBroker.Instance.Unsubscribe("PlayerScored", OnPlayerScored);
```

### Call Types
Events can be published with or without additional data. The framework supports both standard and generic data types for event messages.

#### Example:
```csharp
EventBroker.Instance.Publish("PlayerScored", 100); // with standard data
EventBroker.Instance.Publish("PlayerHealthChanged", new HealthData { CurrentHealth = 80 }); // with custom data type
```

### Data Lifetime Management
Data associated with events can have different lifetimes, allowing for flexible memory management. The following options are available:
- DeleteAfterUse: The data is removed after it is accessed. This is useful for temporary data that does not need to persist beyond a single use.
- DeleteByCommand: The data remains available until explicitly removed by the developer. This gives more control over data management but requires careful handling to avoid memory leaks.
- DoNotDelete: The data persists indefinitely until the developer decides to manage it. This is useful for situations where data must be retained for long periods.

#### Lifecycle of Invokable Data
The lifecycle of invokable data involves several steps:

1. Preparation(optional): Data is prepared for the next event using PrepareInvokableDataForNextEvent, where you define the data and its lifetime.
  Note: Prepared data will only be bound to the next event; ensure that you manage your data carefully.
2. Publishing: The event is published using Publish, which triggers any subscribed handlers.
3. Subscription: Subscribers can subscribe to the event to react to the published data.
4. Retrieval: Subscribers retrieve the prepared data using GetInvokableData, allowing them to access the information needed for their logic.
5. Clarification(optional): If necessary, the invocation data can be updated or clarified using ClarifyInvokationData to ensure it reflects the current state.
6. Removal(optional): Finally, if the data is no longer needed, it can be removed using RemoveInvokableData.
7. Unsubscription: After processing, the subscriber can unsubscribe to avoid further notifications.

#### Example of Data Lifetime Management and Lifetime Management:
```csharp
// Subscribe to the event
EventBroker.Instance.Subscribe("PlayerStatusUpdated", HandlePlayerStatusUpdated);

// Prepare data for the next event
EventBroker.Instance.PrepareInvokableDataForNextEvent("PlayerStatus", new HealthData { CurrentHealth = 100 }, DataLifetime.DeleteAfterUse);

// Publish the event
EventBroker.Instance.Publish("PlayerStatusUpdated");

// Method to handle the event
void HandlePlayerStatusUpdated(EventArgs args)
{
    var playerHealth = EventBroker.Instance.GetInvokableData<HealthData>(args.ID, "PlayerStatus");
    UpdatePlayerHealthUI(playerHealth.CurrentHealth);

    // Clarify the invocation data if needed
    EventBroker.Instance.ClarifyInvokationData(args.ID, "PlayerStatus", new HealthData { CurrentHealth = 90 });

    // Optionally, delete the data if necessary
    EventBroker.Instance.RemoveInvokableData(args.ID, "PlayerStatus");

    // Unsubscribe after processing
    EventBroker.Instance.Unsubscribe("PlayerStatusUpdated", HandlePlayerStatusUpdated);
}
```

### Generic and Standard Data Types
The Event Broker supports both generic and standard data types, allowing developers to publish any type of data with events. This flexibility makes it easy to work with various data structures within your game.

#### Example:
```csharp
// Using a standard data type
EventBroker.Instance.Publish("GamePaused", true); // with a standard boolean type

// Using a generic data type
EventBroker.Instance.Publish("PlayerHealthChanged", new HealthData { CurrentHealth = 80 });
```

### Using Tags
Tags are a mechanism to categorize and differentiate data associated with events. By using tags, developers can manage multiple instances of similar data types without conflict.

#### Example of Using Tags:
When using the same type of data with different tags for a single event, you can effectively manage multiple states or instances of that data.

```csharp
// Prepare the same type of data with different tags for the same event
EventBroker.Instance.PrepareInvokableDataForNextEvent("PlayerStatus1", new HealthData { CurrentHealth = 100 }, DataLifetime.DeleteByCommand);
EventBroker.Instance.PrepareInvokableDataForNextEvent("PlayerStatus2", new HealthData { CurrentHealth = 50 }, DataLifetime.DeleteByCommand);

// Subscribe to the event and handle the data in the callback
EventBroker.Instance.Subscribe("PlayerStatusUpdated", OnPlayerStatusUpdated);

private void OnPlayerStatusUpdated(EventArgs args)
{
    // Retrieve data based on tags from the event arguments
    var playerHealth1 = EventBroker.Instance.GetInvokableData<HealthData>(args.ID, "PlayerStatus1");
    var playerHealth2 = EventBroker.Instance.GetInvokableData<HealthData>(args.ID, "PlayerStatus2");

    UpdatePlayerHealthUI(playerHealth1.CurrentHealth, playerHealth2.CurrentHealth);
}
```

## Usage Examples
Here are some common use cases for the Event Broker:

1. Game State Management:
```csharp
EventBroker.Instance.Publish("GameStarted");
EventBroker.Instance.Subscribe("GameStarted", () => StartGame());
```
2. UI Updates:
```csharp
EventBroker.Instance.Publish("ScoreUpdated", new ScoreData { CurrentScore = 150 });
EventBroker.Instance.Subscribe("ScoreUpdated", (args) => 
{
    var scoreData = (ScoreData)args.Message;
    UpdateScoreUI(scoreData.CurrentScore);
});
```
3. Decoupled Communication:
```csharp
EventBroker.Instance.Publish("PlayerDied");
EventBroker.Instance.Subscribe("PlayerDied", () => HandlePlayerDeath());
```
4. Player Status Updates:
```csharp
EventBroker.Instance.PrepareInvokableDataForNextEvent("PlayerStatus1", new HealthData { CurrentHealth = 70 }, DataLifetime.DeleteByCommand);
EventBroker.Instance.PrepareInvokableDataForNextEvent("PlayerStatus2", new HealthData { CurrentHealth = 30 }, DataLifetime.DeleteByCommand);
EventBroker.Instance.Publish("PlayerStatusUpdated");
```

## Installation
To integrate the Event Broker into your Unity project:

1. Clone the repository or download the source code.
2. Add the EventBroker.cs file to your Unity project.
3. Start using the Event Broker by calling EventBroker.Instance in your scripts.

## Contributing
Contributions are welcome! Please feel free to submit pull requests or file issues for any bugs or feature requests.

## License
This project is licensed under the MIT License. See the LICENSE file for details.
