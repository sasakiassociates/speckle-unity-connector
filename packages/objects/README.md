# speckle-unity-objects

An unofficial set of speckle converters and objects built for unity. This project was started from the
main [speckle-unity](https://github.com/specklesystems/speckle-unity) project, but has been moved into different repos
for more modular package support with unity.

This package features:

- `ConverterUnity` - The main scriptable speckle converter
- A basic list of component converters that handle main speckle objects in unity
- A basic set of unity components for creating speckle objects in unity.

If you are looking for a more complete unity pacakge take a look
at [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)

This pacakge can be installed with [OpenUpm](https://github.com/openupm/openupm-cli#installation)

`openupm add com.speckle.objects`

## Roadmap

> This pacakge is in active development, please be aware of any breaking changes if you plan on using this pacakge. (
> sorry for the bad packaging tags)

| Version | Defining Feature|
| ------- | -------------------------------------------------------------------------------- |
| ~1.1.1~   | ~Package structure~|
| ~1.1.2~   | ~Deconstruct code from main repo for objects and converters~|
| ~1.1.3~   | ~Mesh Converter~|
| ~1.1.4~   | ~Line Converter~|
| ~1.1.5~   | ~Brep Converter~ |
| ~1.1.6~   | ~Point Converter~|
| ~1.1.7~   | ~Default Converter~ |
| 1.1.8     | Point Cloud Converter|
| 1.1.9     | Basic BIM Converter  |
| 1.2.0     | Block Converter |

### Additional

There are additional packages in active development.

- Main client and operations package  
  [speckle-unity-core](https://github.com/sasakiassociates/speckle-unity-core)

- Core and Objects wrapped together with some additional GUI, probably the more ideal package to install
  [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)
