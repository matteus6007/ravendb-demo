# RavenDB 3.5 Instructions

Demo for managing data in a Raven `3.5` Database.

* [Installation](#installation)
* [Admin](#admin)
* [Export Data](#export-data)
  * [Management Studio](#management-studio)
  * [CLI](#cli)
* [Changes API](#changes-api)
* [Data Subscriptions](#data-subscriptions)
  * [Criteria](#criteria)
  * [Strategies](#strategies)
  * [Running the Application](#running-the-application)

## Installation

Download latest stable release from https://ravendb.net/download.

Running RavenDB version `3.5` locally running on port `8080`:

```shell
<path_to_ravendb>/Server/Raven.Server.exe
```

You can access RavenDB instance on `http://localhost:8080`.

## Admin

See https://ravendb.net/docs/article-page/3.5/csharp/studio/accessing-studio.

## Export Data

### Management Studio

See https://ravendb.net/docs/article-page/3.5/csharp/file-system/studio/tasks/export-and-import-views.

Go to `Databases > Mobile > Tasks > Export Database`.

Select `Advanced > Collections > Specified collections only` if you want to export specific collections.

### CLI

Use the [Smuggler](https://ravendb.net/docs/article-page/3.5/csharp/server/administration/exporting-and-importing-data) utility.

_Note: You now need to separately download the `Tools` package._

Run command:

```shell
Raven.Smuggler out http://localhost:8080 raven.dump --operate-on-types=Documents --database="Mobile" --batch-size=1024 --excludeexpired --metadata-filter=Raven-Entity-Name="MobileDevices"
```

To view the output you can extract the `raven.dump` file to JSON:

```json
{
  "Docs": [
    {
      "name": "Galaxy S24",
      "family": "Samsung",
      "storage": "128GB",
      "colour": "Black",
      "createdOn": "2023-01-01 09:00:00",
      "updatedOn": "2023-01-01 09:00:00",
      "@metadata": {
        "Raven-Entity-Name": "MobileDevices",
        "Raven-Replication-Merged-History": true,
        "Raven-Replication-History": [
          {
            "Raven-Replication-Version": 2,
            "Raven-Replication-Source": "b95cda07-9a29-4282-94a0-3b024e727113"
          }
        ],
        "Raven-Replication-Source": "b95cda07-9a29-4282-94a0-3b024e727113",
        "Raven-Replication-Version": 3,
        "@id": "1234",
        "Last-Modified": "2023-11-01T14:43:12.5663575Z",
        "Raven-Last-Modified": "2023-11-01T14:43:12.5663575",
        "@etag": "01000000-0000-0001-0000-000000000003",
        "Non-Authoritative-Information": false
      }
    }
  ],
  "Attachments": [],
  "Indexes": [],
  "Transformers": [],
  "Identities": []
}
```

_Note: Truncated for brevity._

## Changes API

[Changes API](https://ravendb.net/docs/article-page/3.5/csharp/client-api/changes/how-to-subscribe-to-document-changes) is a Push Notification service, that allows a client to subscribe to document changes.

_Note: the change doesn't include current state only the ID and Collection Name, you need to load the document to view the latest version._

Returns the following information:

* `Type` - type of change such as `PUT` or `DELETE`
* `Id` - document identifier
* `CollectionName` - document's collection name
* `TypeName` - type name  _(always seems to be null)_
* `Etag` - document etag
* `Message` - notification payload _(always seems to be null)_

Run test app to listen to changes to `MobileDevices` collection:

```shell
dotnet run --project .\src\Raven35.Subscriptions\Raven35.Subscriptions.ConsoleApp\Raven35.Subscriptions.ConsoleApp.csproj Changes
```

## Data Subscriptions

[Data subscriptions](https://ravendb.net/docs/article-page/3.5/csharp/client-api/data-subscriptions/what-are-data-subscriptions) provide a reliable and handy way to retrieve documents from the database for processing purposes by application jobs.

Returns batches of `RavenJObject` or type `T` objects.

_Note: No way to determine type of change, it is merely the current state of the document, so you cannot subscribe to document deletions. However, you can use the [Changes API](#changes-api) to subscribe to document delete notifications._

### Criteria

Define a `SubscriptionCriteria` or `SubscriptionCriteria<T>` with the following optional filters:

* `KeyStartsWith` - a document id must starts with a specified prefix
* `BelongsToAnyCollection` - list of collections that the subscription deals with - _note: implied for `SubscriptionCriteria<T>`_
* `PropertiesMatch` - dictionary of field names and related values that a document must have
* `PropertiesNotMatch` - dictionary of field names and related values that a document must not have
* `StartEtag` - an etag of a document which a subscription is going to consider as already acknowledged and start processing docs with higher etags

### Strategies

* `OpenIfFree` _(default)_ - open a subscription only if there isn't any other currently connected client, throws `SubscriptionInUseException` if there is an open connection
* `TakeOver` - successfully open a subscription even if there is another currently connected client, open connection will be closed
* `ForceAndKeep` - keeps connection until another client with `ForceAndKeep` requests access
* `WaitForFree` - if there is an open connection, waits for connection to be disconnected

### Running the Application

Run test app to listen to changes to `MobileDevices` collection using the `TakeOver` strategy:

```shell
dotnet run --project .\src\Raven35.Subscriptions\Raven35.Subscriptions.ConsoleApp\Raven35.Subscriptions.ConsoleApp.csproj Data
```

Get list of subscriptions for `Mobile` database:

```shell
curl http://localhost:8080/databases/Mobile/subscriptions
```

Outputs:

```json
[
    {
        "SubscriptionId": 1,
        "Criteria": {
            "KeyStartsWith": null,
            "StartEtag": "01000000-0000-0001-0000-000000000002",
            "BelongsToAnyCollection": [
                "MobileDevices"
            ],
            "PropertiesMatch": null,
            "PropertiesNotMatch": null
        },
        "AckEtag": "01000000-0000-0008-0000-000000000001",
        "TimeOfSendingLastBatch": "2023-11-14T10:56:46.9358120Z",
        "TimeOfLastAcknowledgment": "2023-11-14T10:56:47.0182839Z",
        "TimeOfLastClientActivity": "2023-11-14T10:56:57.3034535Z"
    }
]
```