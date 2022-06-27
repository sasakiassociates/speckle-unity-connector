# speckle-unity-core

An unoffical client connection for using speckle in unity. This project was started from the
main [speckle-unity](https://github.com/specklesystems/speckle-unity) project, but has been moved into different repos
for more modular package support with unity.

This package features:

- `SpeckleStream` - A scriptable object for storing and accessing speckle streams
- `Sender` + `Receiver` - the main operation objects that handle interacting with speckle and unity
- `ScriptableConverter` - A wrapper object for `ISpeckleConverter` that targets more unity specific stuff
- `ComponentConverter<>`- a modular scriptable object for customising how certain speckle objects convert into unity

If you are looking for a more complete unity pacakge take a look
at [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)

This pacakge can be installed with [OpenUpm](https://github.com/openupm/openupm-cli#installation)

`openupm add com.speckle.core`

## Roadmap

> These pacakges are in active development, please be aware of any breaking changes if you plan on using this pacakge.

| Version | Defining Feature|
| ------- | -------------------------------------------------------------------------------- |
| ~1.0.1~   | ~Package structure~|
| ~1.0.2~   | ~Deconstruct code from main repo for Converter and Clients~|
| ~1.0.3~   | ~Receiving Async Commands for Editor and Runtime~|
| ~1.0.4~   | ~Static commands for client commands~|
| ~1.0.5~   | ~Receive Speckle Commit as a Speckle Node~ |
| ~1.0.6~   | ~Sending Async Commands for Editor and Runtime with Speckle Node~|
| ~1.0.7~   | ~Speckle Node and Layer object for maintaing commit data~ |
| 1.0.8     | Editor Support for Speckle Node and Speckle Layers|
| 1.0.9     | Tree and List support for Speckle Node |
| 1.1.0     | Client Subscriptions as Unity Events |

## Additional

There are additional packages in active development.

- Supported objects and conversions
  [speckle-unity-objects](https://github.com/sasakiassociates/speckle-unity-objects)

- Core and Objects wrapped together with some additional GUI, probably the more ideal package to install
  [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)

