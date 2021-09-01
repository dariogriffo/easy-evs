
[![NuGet Info](https://buildstats.info/nuget/EasyEvs?includePreReleases=true)](https://www.nuget.org/packages/EasyEvs/)
[![GitHub license](https://img.shields.io/github/license/griffo-io/easy-evs.svg)](https://raw.githubusercontent.com/griffo-io/easy-evs/master/LICENSE)

[![N|Solid](https://avatars2.githubusercontent.com/u/39886363?s=200&v=4)](https://github.com/griffo-io/easy-evs)

# EasyEvs

A lightweight developer driven application framework to work with Greg Young's [EventStore](https://eventstore.com/).

## Table of contents

- [About](#about)
- [Getting Started](#getting-started)
- [Examples](#examples)
- [License](#license)

## About

[EasyEvs](https://www.nuget.org/packages/EasyEvs) is a light application framework for Greg Young's [EventStore](https://eventstore.com/).

After having been working with EventStore for some time now, and finding different issues on the code using it, starting from the Aggregate roots down to connection management, EasyEvs was born as an effort to allow me start new projects quickly on top of EventStore.
The motivation behind it is to allow other developers of different levels to use EventStore with the minimum of effort, making simple to configure the behavior, and allow unit testing of simple things (routing, writing and reading).
Also the framework is split in 2 nuget packages to allow easy unit testing without installing all the EventStore dependencies in domain projects.
***EasyEvs*** is not meant to evolve to a heavy bloated library, but to keep things simple and working. Is focused on microservices architecture where [DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) is not much of an issue.
***EasyEvs*** main purpose is to allow you to work with EventStore in a few lines, and that's how it will always be.

### Who is it for?

[EasyEvs](https://www.nuget.org/packages/EasyEvs) is intended for developers who want to work with [event sourcing](https://www.eventstore.com/blog/what-is-event-sourcing) and a reliable event store, simplifying their code and life.

It is not designed to be deal with every case, but the simple ones, the ones you will be doing 99% of the time.
It `enforces` the implementation of some interfaces with the aim of having a consistent development experience, so nobody has to worry about the basics of your event store.

## Getting Started

### With Contracts implementation
`Install-Package EasyEvs.Contracts`

- Define your events and make them implement [IEvent](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IEvent.cs#L8).
	- Suggestion: place them into a their own project and make a nuget package out of it to share with the other teams.
- Implement the [`IStreamResolver`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IStreamResolver.cs#L9) interface to allow the [`IEventStore`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IEventStore.cs#L12) know where to append your events
- Implement an [`IHandlesEvent`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IHandlesEvent1.cs#L11)

That's it you can start coding, and unit testing... Now you want to see if against a real EventStore instance?

### Integrating with a real instance
`Install-Package EasyEvs`

- Add a configuration section called `EasyEvs` and configure the EventStore with the settings class [`EventStoreSettings`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs/EventStoreSettings.cs#L10) with the only required property `ConnectionString`
- Add EasyEvs to your ServiceCollection (in your writer service and in your reader service) via the extension method [`services.AddEasyEvs(...)`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs/ServiceCollectionExtensions.cs#L25)
- Subscribe to a stream or projection on the EventStore with [`SubscribeToStream(...)`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IEventStore.cs#L92)
- Append your events to the EventStore with [`Append(..)`](https://github.com/griffo-io/easy-evs/main/src/EasyEvs.Contracts/IEventStore.cs#L22)
 
 That's it, the simplest way to start event sourcing
 
## Examples

A Publisher and Subscriber can be found [here](https://github.com/griffo-io/easy-evs/tree/main/examples) 
## License

[Apache 2.0](https://github.com/griffo-io/easy-evs/blob/main/LICENSE)
