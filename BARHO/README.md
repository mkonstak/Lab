# BARHO - Laboratory Calculation Tool

BARHO is a high-precision colorimetric calculation application built with .NET MAUI. It is designed for laboratory use to transform spectral coordinates into Hue (Odstín) and Saturation (Sytost) values using advanced interpolation algorithms.

## Features

- **Precise Calculations**: Utilizes a hardcoded 3x27 spectral boundary matrix for reliable data mapping.
- **Automated Workflow**: Optimized for rapid data entry with a full keyboard-only cycle using the **Enter** key.
- **Premium Interface**: Modern dark-themed UI with subtle gradients and smooth animations.
- **Focus Retention**: Automatically regains focus on action buttons when returning to the application (Alt+Tab friendly).
- **Localized Output**: Results are formatted according to Central European standards (comma as decimal separator) and rounded to two decimal places.

## How to Use (Keyboard Workflow)

The application is designed for speed. You can perform a full measurement cycle without using a mouse:

1. **Input X**: Enter the X coordinate and press **Enter**. Focus moves to the next field automatically.
2. **Input Y**: Enter the Y coordinate and press **Enter**. The application calculates results instantly.
3. **View Results**: Results appear with a smooth fade-in animation.
4. **Next Sample**: The **"DALŠÍ VZOREK?"** button appears and gains focus. Press **Enter** again to reset the form and return to the first field.

## Technical Specifications

- **Framework**: .NET 10.0 MAUI (utilizing WinUI 3 for the Windows desktop implementation).
- **Target Platform**: Windows 10 (version 1809 / build 17763) or higher.
- **Architecture**: `win-x64` (64-bit).
- **Deployment Model**: **Unpackaged & Self-Contained**. The application includes the full .NET Runtime and Windows App SDK, allowing it to run as a portable "green" application without installation or pre-requisite runtimes.
- **Localization**: Specifically configured for the **cs-CZ (Czech)** culture. The UI uses the comma (`,`) as the decimal separator for inputs and outputs, aligned with local laboratory standards.

## Mathematical Algorithm

BARHO implements a high-precision colorimetric transformation algorithm to convert trichromatic coordinates ($X, Y$) into Hue (**Odstín**) and Saturation (**Sytost**).

### Core Logic:

1. **Normalization**: Input coordinates are normalized against a reference white point ($X_u=98.07, Y_u=100, Z_u=118.22$, typical for $D_{65}$ illuminant).
2. **Spectral Boundary Mapping**: The app utilizes a hardcoded 3x27 spectral boundary matrix.
3. **Cubic Spline Interpolation**: To find points between the discrete matrix values, the algorithm uses a four-point window cubic interpolation (Cubic Spline) to ensure a smooth transition along the spectral curve.
4. **Binary Search**: A precision binary search (12 iterations) is performed on the interpolated curve to locate the exact segment corresponding to the input coordinates.
5. **Coordinate Transformation**: Final results are transformed through the $u', v'$ color space to calculate the saturation ratio.

## Project Structure & Key Files

- **[BARHO.csproj](BARHO.csproj)**: Project configuration. Note the `<WindowsPackageType>None</WindowsPackageType>` setting which enables the unpackaged (portable) distribution mode.
- **[MainPage.xaml](MainPage.xaml)**: UI definition using XAML. Employs a dark theme with `LinearGradientBrush` backgrounds and `Label` animations for result display.
- **[MainPage.xaml.cs](MainPage.xaml.cs)**: The "brain" of the application.
  - `OnCalculateClicked`: Orchestrates the math logic described above.
  - `OnInputXCompleted`/`OnInputYCompleted`: Implements the automated keyboard workflow.
  - `OnWindowActivated`: Logic to ensure focus is regained when the user switches back to the app.
- **[Resources/](Resources/)**: Contains application assets including fonts and the splash screen.

## Development & Build Information

### Prerequisites

- **.NET 10.0 SDK**.
- **MAUI Workload**: Installed via `dotnet workload install maui-windows`.

### Building from Source

To rebuild the application as a single self-contained directory for distribution:

```powershell
dotnet publish -f net10.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:WindowsPackageType=None
```

The resulting executable and its dependencies will be located in `bin/Release/net10.0-windows10.0.19041.0/win-x64/publish/`.

## Deployment & Installation

1. Copy the contents of the `publish` folder to the target machine.
2. Run `BARHO.exe`.

No installation or administrative privileges are required.
