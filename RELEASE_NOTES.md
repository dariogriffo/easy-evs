Release Notes
=============

## [4.1.0](https://github.com/griffo-io/easy-evs/releases/tag/4.1.1)

Fix read retry.

## [4.1.0](https://github.com/griffo-io/easy-evs/releases/tag/4.1.0)

Added basic retry mechanism.

## [4.0.0](https://github.com/griffo-io/easy-evs/releases/tag/4.0.0)

Load events from assembly without full qualified name

## [3.2.2](https://github.com/griffo-io/easy-evs/releases/tag/3.2.2)

Use polymorphic serialization

## [3.2.1](https://github.com/griffo-io/easy-evs/releases/tag/3.2.1)

Patch Contracts version

## [3.2.0](https://github.com/griffo-io/easy-evs/releases/tag/3.2.0)

Added Create for AggregateRoots to ensure stream doesn't exist

## [3.1.0](https://github.com/griffo-io/easy-evs/releases/tag/3.1.0)

Allow simple configuration.

## [3.0.0](https://github.com/griffo-io/easy-evs/releases/tag/3.0.0)

Simplify interfaces including the metadata on the IEvent and removed the IEnrichedEvent.

## [2.1.1](https://github.com/griffo-io/easy-evs/releases/tag/2.1.1)

Don't serialize metadata for IEnrichedEvent if present as part of the data

## [2.1.0](https://github.com/griffo-io/easy-evs/releases/tag/2.1.0)

Ensure pre actions, post actions and the handler are created when required to ensure any previous actions in the pipeline may setup something

## [2.0.2](https://github.com/griffo-io/easy-evs/releases/tag/2.0.2)

Added another possible condition for disposed error [different from documentation](https://github.com/EventStore/EventStore-Client-Dotnet/issues/154)

## [2.0.1](https://github.com/griffo-io/easy-evs/releases/tag/2.0.1)

Fixed failure on dropped subscription on client disconnected by the user

## [2.0.0](https://github.com/griffo-io/easy-evs/releases/tag/2.0.0)

Split [Contracts](https://www.nuget.org/packages/EasyEvs.Contracts) and Implementation into different nuget packages to allow easy Unit testing without bloating with dependencies

## [1.1.0](https://github.com/griffo-io/easy-evs/releases/tag/1.1.0)

Added Pre and Post actions


## [1.0.0](https://github.com/griffo-io/easy-evs/releases/tag/1.0.0)

First tested stable version

Fixed examples


## [0.5.0](https://github.com/griffo-io/easy-evs/releases/tag/0.5.0)

Adding tests and fixing issues


## [0.4.0](https://github.com/griffo-io/easy-evs/releases/tag/0.4.0)

Added aggregate roots and logic to save/restore from the Event Store


## [0.3.0](https://github.com/griffo-io/easy-evs/releases/tag/0.3.0)

Adding SubscribeCommand to allow treating missing handlers independently

Fixed empty package


## [0.2.0](https://github.com/griffo-io/easy-evs/releases/tag/0.2.0)

Remove unnecessary classes.

Added XML documentation to all exported types.


## [0.1.0](https://github.com/griffo-io/easy-evs/releases/tag/0.1.0)

First release with a basic set of functionality.
