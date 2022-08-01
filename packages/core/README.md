# speckle-unity-core

[![openupm](https://img.shields.io/npm/v/com.speckle.core?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.speckle.core/)


A unity package that adapts the Speckle Core functionality into unity editor and runtime. Some spiffy lil features to note:


- `Speckle Unity Client` - A simple object that handles speckle client connections in unity
- `Client Behaviour` - A monobehaviour object for adding speckle functionality to game objects 
- `Sender` + `Receiver` - the main operation objects that handle interacting with speckle and unity
- `SpeckleOps` - A static wrapper for operational calls with Speckle Sharp
- `SpeckleStream` - A scriptable object for storing and accessing speckle streams
- `ScriptableConverter` - A wrapper object for `ISpeckleConverter` that targets more unity specific stuff
- `ComponentConverter<>`- a modular scriptable object for customising how certain speckle objects convert into unity

If you are looking for a more complete unity package take a look
at [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)

This package can be installed with [OpenUpm](https://github.com/openupm/openupm-cli#installation)

`openupm add com.speckle.core`