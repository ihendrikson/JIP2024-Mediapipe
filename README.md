# JIP 2024 Ethereal Matter

## Installation
If you don't have git installed, you can install it from [here](https://git-scm.com/downloads). The project can then be cloned (or downloaded):
```
git clone --recurse-submodules git@github.com:ihendrikson/JIP2024-Mediapipe.git
```

### Install the Orbbec SDK for python (pyorbbecsdk)
Make sure you have [CMake](https://cmake.org/download/), [Visual Studio](https://visualstudio.microsoft.com/),  [Python](https://www.python.org/downloads/) (tested with 3.10) installed with [Pip](https://pip.pypa.io/en/stable/installation/). It is advised to use a separate environment, you can to this using [miniconda](https://docs.anaconda.com/miniconda/) If you are using miniconda, use the base environment. You can convert the Python version of your miniconda base environment:
```
conda install python=3.10
```
Go to the pyorbbecsdk directory from the project repository and install the requirements:
```
cd pyorbbecsdk
pip install -r requirements.txt
pip install mediapipe
```
Prepare the directory and open cmake (from conda prompt):
```
mkdir build
cd build
cmake-gui
```
Running cmake form your conda assigns the correct Python and Pybind version (still check to make sure). Make sure that your source code directory is `path/to/repo/JIP2024-Mediapipe/pyorbbecsdk` and the build binary directory is `path/to/repo/JIP2024-Mediapipe/pyorbbecsdk/build`. You can then press `configure` and then `generate`. Then press `Open Project` to open the project in Visial studio.
In Visual Studio right click on the `pyorbbecsdk` from the solution explorer, and press `rebuild`. Then do the same for `INSTALL`. 

Now open your explorer and go to the project directory. Go to the folder `pyorbbecsdk/install/lib`. Copy all the files and paste them in `pyorbbecsdk/examples`.

As a final step you need enable Device Timestamps via UVC Protocol on Windows:
1. Connect the device
2. Open Powershell as an administrator
3. Go to the `scripts` directory from the `pyorbbec` folder
4. Modify the execution policy by running `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`, then press `Y`
5. Execute the registration script by running `.\obsensor_metadata_win10.ps1 -op install_all`

If you encounter any problems or use Mac or Linux, please refer to the [official install instructions](https://github.com/orbbec/pyorbbecsdk?tab=readme-ov-file) 
## Running the project
In order to connect Mediapipe to Unity, a python script needs to run on the local sevice which will send the estimated pose over a UDP server to unity.
To run the Python script:
```
cd JIP2024-Mediapipe
cd mediapipeavatar
python main.py
```

You can then open the project and Unity. This can be done by opening the `UnityMediaPipeAvatar` directory. You can open the calibration scene under `Assets/Scenes/CalibrationScene`
